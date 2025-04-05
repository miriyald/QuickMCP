using System.Text;
using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Types;

namespace AutoMCP.Http;

// Class responsible for making HTTP API calls.  This class is designed to be
// reusable and efficient, using a single HttpClient instance for all calls.
public class HttpApiCaller
{
    private readonly HttpClient _httpClient;
    private IAuthenticator? _authenticator;
    private ILogger? _logger;
    string _baseUrl;

    private Dictionary<string, string> _defaultPathParams = new Dictionary<string, string>();

    public HttpApiCaller(HttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
    }


    public void SetAuthenticator(IAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void SetDefaultPathParams(Dictionary<string, string> defaultPathParams)
    {
        _defaultPathParams = defaultPathParams;
    }

    public async Task<CallToolResponse> Call(HttpMethod method, string url,
        Dictionary<string, JsonElement>? content = null, string mimeType = "application/json",
        List<KeyValuePair<string, string?>>? query = null, Dictionary<string, string?>? pathParam = null,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(url, query, pathParam);
        _logger?.LogInformation($"Making {method} request to {url}");


        HttpRequestMessage request = new HttpRequestMessage(method, uri);


        AddContent(request, content, mimeType);

        if (_authenticator != null)
        {
            await _authenticator.AuthenticateRequestAsync(request);
        }

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        #if NETSTANDARD2_0_OR_GREATER
          string responseContent = await response.Content.ReadAsStringAsync();
        #else
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        #endif
      

        _logger?.LogDebug($"Response from {url}: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError($"HTTP error {response.StatusCode} from {url}: {responseContent}");
            // improved error handling.  Include the response content
            return new CallToolResponse()
            {
                IsError = true,
                Content = new List<Content>([
                    new Content()
                    {
                        Text = $"HTTP error {response.StatusCode} from {url}: {responseContent}"
                    }
                ])
            };
        }

        try
        {
            return new CallToolResponse()
            {
                Content = new List<Content>([
                    new Content()
                    {
                        Type = response.Content.Headers.ContentType?.MediaType ?? "",
                        Text = responseContent,
                    }
                ])
            };
        }
        catch (JsonException ex)
        {
            // If the response is not a valid JsonRpcResponse, create an error.
            _logger?.LogError(
                $"Error deserializing response from {url}: {ex.Message}.  Response was: {responseContent}");
            return new CallToolResponse()
            {
                IsError = true,
                Content = new List<Content>([
                    new Content()
                    {
                        Text =
                            $"Error deserializing response from {url}: {ex.Message}.  Response was: {responseContent}"
                    }
                ])
            };
        }
    }

    private void AddContent(HttpRequestMessage request, Dictionary<string, JsonElement>? content, string mimeType)
    {
        if (content != null)
        {
            if (mimeType == "application/json")
            {
                request.Content =
                    new StringContent(
                        JsonSerializer.Serialize(content,
                            AutoMcpJsonSerializerContext.Default.DictionaryStringJsonElement), Encoding.UTF8, mimeType);
            }
            else if (mimeType == "application/x-www-form-urlencoded")
            {
                request.Content = new FormUrlEncodedContent(content.Select(x =>
                    new KeyValuePair<string, string>(x.Key, x.Value.GetRawText())));
            }
            else if (mimeType == "multipart/form-data")
            {
                var multipart = new MultipartFormDataContent();
                foreach (var value in content)
                {
                    multipart.Add(new StringContent(value.Value.GetRawText()), value.Key);
                }

                request.Content = multipart;
            }
            else throw new Exception($"Unsupported mime type {mimeType}");
        }
    }

    private Uri BuildUri(string url, List<KeyValuePair<string,string?>>? query, Dictionary<string, string?>? pathParam)
    {
        if (_baseUrl.EndsWith("/"))
        {
            _baseUrl = _baseUrl.Substring(0, _baseUrl.Length - 1);
        }

        //Replace Default Path Params
        foreach (var param in _defaultPathParams)
        {
            if (string.IsNullOrEmpty(param.Value)) continue;
            url = url.Replace($"{{{param.Key}}}", param.Value ?? string.Empty);
        }

        //Replace path params
        if (pathParam != null)
        {
            foreach (var param in pathParam)
            {
                if (string.IsNullOrEmpty(param.Value)) continue;
                url = url.Replace($"{{{param.Key}}}", param.Value ?? string.Empty);
            }
        }

        if (!url.StartsWith($"/"))
        {
            url = $"/{url}";
        }

        var urlBuilder = new UriBuilder(_baseUrl + url);
        if (query != null)
        {
            urlBuilder.Query = string.Join("&",
                query?.Select(
                    kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}") ??
                Array.Empty<string>());
        }

        return urlBuilder.Uri;
    }
}
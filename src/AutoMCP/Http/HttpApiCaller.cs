using System.Text;
using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMCP.Http;

// Class responsible for making HTTP API calls.  This class is designed to be
// reusable and efficient, using a single HttpClient instance for all calls.
public class HttpApiCaller
{
    private readonly HttpClient _httpClient;
    private IAuthenticator? _authenticator;
    private ILogger? _logger;

    public HttpApiCaller(HttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }


    public void SetAuthenticator(IAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }


    public async Task<JsonRpcResponse> Call(HttpMethod method, string url, StringContent content = null,
        Dictionary<string, string> headers = null)
    {
        _logger?.LogInformation($"Making {method} request to {url}");
        HttpRequestMessage request = new HttpRequestMessage(method, url);

        if (content != null)
        {
            request.Content = content;
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (_authenticator != null)
        {
            await _authenticator.AuthenticateRequestAsync(request);
        }

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        string responseContent = await response.Content.ReadAsStringAsync();

        _logger?.LogDebug($"Response from {url}: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError($"HTTP error {response.StatusCode} from {url}: {responseContent}");
            // improved error handling.  Include the response content
            return new JsonRpcResponse
            {
                Error = new JsonRpcError()
                {
                    Message = $"HTTP error {response.StatusCode}: {responseContent}",
                    Code = (int)response.StatusCode
                }
            };
        }

        try
        {
            // Attempt to deserialize the response as a JsonRpcResponse.
            JsonRpcResponse jsonResponse = JsonSerializer.Deserialize<JsonRpcResponse>(responseContent);
            return jsonResponse;
        }
        catch (JsonException ex)
        {
            // If the response is not a valid JsonRpcResponse, create an error.
            _logger?.LogError(
                $"Error deserializing response from {url}: {ex.Message}.  Response was: {responseContent}");
            return new JsonRpcResponse
            {
                Error = new JsonRpcError()
                {
                    Message = $"Invalid JSON response: {ex.Message}.  Response was: {responseContent}",
                    Code = -1
                }
            };
        }
    }
}
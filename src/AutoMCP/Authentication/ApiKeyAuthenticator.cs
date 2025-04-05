using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides API key authentication (in header or query parameter)
/// </summary>
public class ApiKeyAuthenticator : IAuthenticator
{
    private readonly string _apiKey;
    private readonly string _paramName;
    private readonly ApiKeyLocation _location;

    public enum ApiKeyLocation
    {
        Header,
        QueryParameter
    }

    public ApiKeyAuthenticator(string apiKey, string paramName = "X-API-Key",
        ApiKeyLocation location = ApiKeyLocation.Header)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _paramName = paramName ?? throw new ArgumentNullException(nameof(paramName));
        _location = location;
    }

    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        if (_location == ApiKeyLocation.Header)
        {
            request.Headers.Add(_paramName, _apiKey);
        }
        else
        {
            var uri = request.RequestUri;
            if (uri != null)
            {
                var uriBuilder = new UriBuilder(uri);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query[_paramName] = _apiKey;
                uriBuilder.Query = query.ToString();
                request.RequestUri = uriBuilder.Uri;
            }
        }

        return Task.CompletedTask;
    }

    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        if (_location == ApiKeyLocation.Header)
        {
            return Task.FromResult(new Dictionary<string, string>
            {
                [_paramName] = _apiKey
            });
        }

        return Task.FromResult(new Dictionary<string, string>());
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_apiKey));
    }
}
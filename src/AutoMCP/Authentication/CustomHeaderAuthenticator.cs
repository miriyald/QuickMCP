using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides custom header authentication
/// </summary>
public class CustomHeaderAuthenticator : IAuthenticator
{
    private readonly string _headerName;
    private readonly string _headerValue;

    public CustomHeaderAuthenticator(string headerName, string headerValue)
    {
        _headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        _headerValue = headerValue ?? throw new ArgumentNullException(nameof(headerValue));
    }

    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Add(_headerName, _headerValue);
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            [_headerName] = _headerValue
        });
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_headerValue));
    }
}
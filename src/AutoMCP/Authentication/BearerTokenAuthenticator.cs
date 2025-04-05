using System.Net.Http.Headers;
using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// Provides Bearer token authentication
/// </summary>
public class BearerTokenAuthenticator : IAuthenticator
{
    private readonly string _token;

    public BearerTokenAuthenticator(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_token}"
        });
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_token));
    }
}
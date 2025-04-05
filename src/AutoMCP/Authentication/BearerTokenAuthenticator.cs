using System.Net.Http.Headers;
using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides Bearer token authentication.
/// </summary>
public class BearerTokenAuthenticator : IAuthenticator
{
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenAuthenticator"/> class.
    /// </summary>
    /// <param name="token">The bearer token for authentication.</param>
    /// <exception cref="ArgumentNullException">Thrown if the token is null.</exception>
    public BearerTokenAuthenticator(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_token}"
        });
    }

    /// <inheritdoc />
    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_token));
    }
}
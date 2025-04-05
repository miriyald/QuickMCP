using System.Net.Http.Headers;
using System.Text;
using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides basic authentication with username and password.
/// </summary>
public class BasicAuthenticator : IAuthenticator
{
    private readonly string _username;
    private readonly string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticator"/> class.
    /// </summary>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
    public BasicAuthenticator(string username, string password)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
    }

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
        return Task.FromResult(new Dictionary<string, string>
        {
            ["Authorization"] = $"Basic {authValue}"
        });
    }

    /// <inheritdoc />
    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password));
    }
}
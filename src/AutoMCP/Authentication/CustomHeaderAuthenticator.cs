using AutoMCP.Abstractions;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides custom header authentication.
/// </summary>
public class CustomHeaderAuthenticator : IAuthenticator
{
    private readonly string _headerName;
    private readonly string _headerValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomHeaderAuthenticator"/> class.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The value of the header.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerName"/> or <paramref name="headerValue"/> is null.</exception>
    public CustomHeaderAuthenticator(string headerName, string headerValue)
    {
        _headerName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        _headerValue = headerValue ?? throw new ArgumentNullException(nameof(headerValue));
    }

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Add(_headerName, _headerValue);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            [_headerName] = _headerValue
        });
    }

    /// <inheritdoc />
    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_headerValue));
    }
}
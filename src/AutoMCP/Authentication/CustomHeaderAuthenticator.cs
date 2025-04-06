using AutoMCP.Abstractions;
using AutoMCP.Types;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides custom header authentication.
/// </summary>
public class CustomHeaderAuthenticator : IAuthenticator
{
    #region Fields and Properties
    private readonly string _headerName;
    private readonly string _headerValue;
    public AuthenticatorMetadata Metadata => GetMetadata();
    public string Type => Metadata.Type;

    #endregion
    
    #region Static Methods
    
    /// <summary>
    /// Retrieves the metadata for the API key authenticator, including its name, description,
    /// configuration keys, and type.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticatorMetadata"/> containing details about the API key authenticator.</returns>
    public static AuthenticatorMetadata GetMetadata()
    {
        const string name = "Custom Header Authentication";

        const string description =
        $"Custom Header Authentication, it will add a specified header with a value to the request. ";

        const string type = "customHeader";

        List<(string Key, string Description, bool IsRequired)> configKeys =
        [
            ("headerName", "The name of the header.", true),
            ("headerValue", "The value of the header.", true)
        ];
        return new AuthenticatorMetadata(name, description, configKeys, type);
    }
    
    /// <summary>
    /// Creates a Custom Header Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the header name and value.</param>
    /// <returns>An instance of <see cref="CustomHeaderAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when either 'headerName' or 'headerValue' setting is missing.</exception>
    private static IAuthenticator Create(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("headerName", out var headerName) ||
            !settings.TryGetValue("headerValue", out var headerValue))
        {
            throw new ArgumentException(
                "Custom header authentication requires 'headerName' and 'headerValue' settings");
        }

        return new CustomHeaderAuthenticator(headerName, headerValue);
    }
    #endregion
    
    #region Constructors
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

    #endregion
    
    #region IAuthenticator Implementation
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
    #endregion
}
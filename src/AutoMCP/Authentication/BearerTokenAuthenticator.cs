using System.Net.Http.Headers;
using AutoMCP.Abstractions;
using AutoMCP.Types;

namespace AutoMCP.Authentication;

/// <summary>
/// Provides Bearer token authentication.
/// </summary>
public class BearerTokenAuthenticator : IAuthenticator
{
    #region Fields and Properties

    private readonly string _token;
    
    public string Type => Metadata.Type;
    public AuthenticatorMetadata Metadata => GetMetadata();

    #endregion

    #region Factory

    /// <summary>
    /// Retrieves the metadata for the API key authenticator, including its name, description,
    /// configuration keys, and type.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticatorMetadata"/> containing details about the API key authenticator.</returns>
    public static AuthenticatorMetadata GetMetadata()
    {
        const string name = "Bearer Token Authentication";
        const string description =
        $"Bearer Token Authentication, it will add the token to the `Authorization` header.";

        const string type = "bearerToken";

        List<(string Key, string Description, bool IsRequired)> configKeys =
        [
            ("token", "The bearer token for authentication.", true)
        ];
        return new AuthenticatorMetadata(name, description, configKeys, type);
    }

    /// <summary>
    /// Creates a Bearer Token Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the bearer token.</param>
    /// <returns>An instance of <see cref="BearerTokenAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the 'token' setting is missing.</exception>
    public static IAuthenticator Create(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("token", out var token))
        {
            throw new ArgumentException("Bearer token authentication requires 'token' setting");
        }

        return new BearerTokenAuthenticator(token);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenAuthenticator"/> class.
    /// </summary>
    /// <param name="token">The bearer token for authentication.</param>
    /// <exception cref="ArgumentNullException">Thrown if the token is null.</exception>
    public BearerTokenAuthenticator(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    #endregion

    #region IAuthenticator Implementation

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

    #endregion
}
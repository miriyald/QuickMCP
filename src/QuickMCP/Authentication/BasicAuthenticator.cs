using System.Net.Http.Headers;
using System.Text;
using QuickMCP.Abstractions;
using QuickMCP.Types;

namespace QuickMCP.Authentication;

/// <summary>
/// Provides basic authentication with username and password.
/// </summary>
public class BasicAuthenticator : IAuthenticator
{
    #region Fields and Properties

    private readonly string _username;
    private readonly string _password;
    public string Type => Metadata.Type;
    public AuthenticatorMetadata Metadata => GetMetadata();

    #endregion

    #region Static Methods

    
    /// <summary>
    /// Retrieves the metadata information for the Basic Authenticator.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticatorMetadata"/> containing the name, description, configuration keys, and type for the Basic Authenticator.</returns>
    public static AuthenticatorMetadata GetMetadata()
    {
        const string name = "Basic Authentication";
        const string description =
            $"Basic Authentication, it adds a Base64 encoded username and password to the `Authorization` header.";
        const string type = "basicAuth";

        var configKeys = new List<(string Key, string Description, bool IsRequired)>()
        {
            ("username", "The username for authentication.", true),
            ("password", "The password for authentication.", true)
        };
        return new AuthenticatorMetadata(name, description, configKeys, type);
    }

    /// <summary>
    /// Creates a Basic Authenticator with username and password.
    /// </summary>
    /// <param name="settings">The settings containing the username and password.</param>
    /// <returns>An instance of <see cref="BasicAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when either 'username' or 'password' setting is missing.</exception>
    public static IAuthenticator Create(Dictionary<string, string?> settings)
    {
        if (!settings.TryGetValue("username", out var username) ||
            !settings.TryGetValue("password", out var password))
        {
            throw new ArgumentException("Basic authentication requires 'username' and 'password' settings");
        }

        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new ArgumentException("Basic authentication requires 'username' and 'password' settings");
        
        return new BasicAuthenticator(username!, password!);
    }

    #endregion

    #region Constructors

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

    #endregion

    #region IAuthenticator Implementation

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

    #endregion
}
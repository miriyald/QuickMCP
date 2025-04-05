using AutoMCP.Abstractions;
using AutoMCP.Models;

namespace AutoMCP.Authentication;

/// <summary>
/// Factory for creating authenticators based on configuration.
/// Provides methods to generate various types of authenticators based on the given configuration.
/// </summary>
public static class AuthenticatorFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IAuthenticator"/> based on the provided configuration.
    /// </summary>
    /// <param name="config">The authentication configuration containing type and settings.</param>
    /// <returns>An instance of <see cref="IAuthenticator"/> tailored to the specified authentication type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration or settings are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the authentication type is unsupported or required settings are missing.</exception>
    public static IAuthenticator Create(AuthConfig config)
    {
        if (config == null || config.Settings == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return config.Type.ToLowerInvariant() switch
        {
            "bearer" => CreateBearerTokenAuthenticator(config.Settings),
            "basic" => CreateBasicAuthenticator(config.Settings),
            "oauth2" => CreateOAuth2Authenticator(config.Settings),
            "apiKey" => CreateApiKeyAuthenticator(config.Settings),
            "customHeader" => CreateCustomHeaderAuthenticator(config.Settings),
            _ => throw new ArgumentException($"Unsupported authentication type: {config.Type}")
        };
    }

    /// <summary>
    /// Creates a Bearer Token Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the bearer token.</param>
    /// <returns>An instance of <see cref="BearerTokenAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the 'token' setting is missing.</exception>
    private static IAuthenticator CreateBearerTokenAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("token", out var token))
        {
            throw new ArgumentException("Bearer token authentication requires 'token' setting");
        }

        return new BearerTokenAuthenticator(token);
    }

    /// <summary>
    /// Creates a Basic Authenticator with username and password.
    /// </summary>
    /// <param name="settings">The settings containing the username and password.</param>
    /// <returns>An instance of <see cref="BasicAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when either 'username' or 'password' setting is missing.</exception>
    private static IAuthenticator CreateBasicAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("username", out var username) ||
            !settings.TryGetValue("password", out var password))
        {
            throw new ArgumentException("Basic authentication requires 'username' and 'password' settings");
        }

        return new BasicAuthenticator(username, password);
    }

    /// <summary>
    /// Creates an OAuth2 Client Credentials Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing token URL, client ID, client secret, and optional scope.</param>
    /// <returns>An instance of <see cref="OAuth2ClientCredentialsAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required settings (token URL, client ID, client secret) are missing.</exception>
    private static IAuthenticator CreateOAuth2Authenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("tokenUrl", out var tokenUrl) ||
            !settings.TryGetValue("clientId", out var clientId) ||
            !settings.TryGetValue("clientSecret", out var clientSecret))
        {
            throw new ArgumentException(
                "OAuth2 authentication requires 'tokenUrl', 'clientId', and 'clientSecret' settings");
        }

        settings.TryGetValue("scope", out var scope);

        return new OAuth2ClientCredentialsAuthenticator(tokenUrl, clientId, clientSecret, scope);
    }

    /// <summary>
    /// Creates an API Key Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the API key, optional parameter name, and location.</param>
    /// <returns>An instance of <see cref="ApiKeyAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the 'apiKey' setting is missing.</exception>
    private static IAuthenticator CreateApiKeyAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("apiKey", out var apiKey))
        {
            throw new ArgumentException("API key authentication requires 'apiKey' setting");
        }

        settings.TryGetValue("paramName", out var paramName);
        if (string.IsNullOrEmpty(paramName))
        {
            paramName = "X-API-Key";
        }

        var location = ApiKeyAuthenticator.ApiKeyLocation.Header;
        if (settings.TryGetValue("location", out var locationStr) &&
            locationStr.Equals("query", StringComparison.OrdinalIgnoreCase))
        {
            location = ApiKeyAuthenticator.ApiKeyLocation.QueryParameter;
        }

        return new ApiKeyAuthenticator(apiKey, paramName, location);
    }

    /// <summary>
    /// Creates a Custom Header Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the header name and value.</param>
    /// <returns>An instance of <see cref="CustomHeaderAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when either 'headerName' or 'headerValue' setting is missing.</exception>
    private static IAuthenticator CreateCustomHeaderAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("headerName", out var headerName) ||
            !settings.TryGetValue("headerValue", out var headerValue))
        {
            throw new ArgumentException(
                "Custom header authentication requires 'headerName' and 'headerValue' settings");
        }

        return new CustomHeaderAuthenticator(headerName, headerValue);
    }
}
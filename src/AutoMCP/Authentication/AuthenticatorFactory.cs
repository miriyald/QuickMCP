using AutoMCP.Abstractions;
using AutoMCP.Models;

namespace AutoMCP.Authentication;

/// <summary>
/// Factory for creating authenticators based on configuration
/// </summary>
public static class AuthenticatorFactory
{
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
            "apikey" => CreateApiKeyAuthenticator(config.Settings),
            "customheader" => CreateCustomHeaderAuthenticator(config.Settings),
            _ => throw new ArgumentException($"Unsupported authentication type: {config.Type}")
        };
    }

    private static IAuthenticator CreateBearerTokenAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("token", out var token))
        {
            throw new ArgumentException("Bearer token authentication requires 'token' setting");
        }

        return new BearerTokenAuthenticator(token);
    }

    private static IAuthenticator CreateBasicAuthenticator(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("username", out var username) ||
            !settings.TryGetValue("password", out var password))
        {
            throw new ArgumentException("Basic authentication requires 'username' and 'password' settings");
        }

        return new BasicAuthenticator(username, password);
    }

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
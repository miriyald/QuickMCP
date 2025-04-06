using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMCP.Abstractions;
using AutoMCP.Types;

namespace AutoMCP.Authentication;

/// <summary>
/// Factory for creating authenticators based on configuration.
/// Provides methods to generate various types of authenticators based on the given configuration.
/// </summary>
public static class AuthenticatorFactory
{
    private static readonly Dictionary<string, Func<Dictionary<string, string>, IAuthenticator>>
        _availableAuthenticators =
            new Dictionary<string, Func<Dictionary<string, string>, IAuthenticator>>();

    private static readonly Dictionary<string, AuthenticatorMetadata> _availableAuthenticatorMetadata =
        new Dictionary<string, AuthenticatorMetadata>();

    static AuthenticatorFactory()
    {
        Register(ApiKeyAuthenticator.GetMetadata(), ApiKeyAuthenticator.Create);
        Register(BasicAuthenticator.GetMetadata(), BasicAuthenticator.Create);
        Register(BearerTokenAuthenticator.GetMetadata(), ApiKeyAuthenticator.Create);
        Register(CustomHeaderAuthenticator.GetMetadata(), ApiKeyAuthenticator.Create);
        Register(OAuth2ClientCredentialsAuthenticator.GetMetadata(), ApiKeyAuthenticator.Create);
    }

    /// <summary>
    /// Registers a custom authenticator factory for a specific type.
    /// </summary>
    /// <param name="type">The identifier for the authentication type to associate with the custom factory.</param>
    /// <param name="factory">A function that creates an instance of <see cref="IAuthenticator"/> from a dictionary of settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided type or factory is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type is an empty string.</exception>
    public static void Register(string type, Func<Dictionary<string, string>, IAuthenticator> factory,
        AuthenticatorMetadata? metadata = null)
    {
        _availableAuthenticators[type] = factory;
        if (metadata != null)
            _availableAuthenticatorMetadata[type] = metadata;
    }
    public static void Register(AuthenticatorMetadata metadata, Func<Dictionary<string, string>, IAuthenticator> factory)
    {
        Register(metadata.Type, factory, metadata);
    }

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
            // "bearer" => CreateBearerTokenAuthenticator(config.Settings),
            // "basic" => CreateBasicAuthenticator(config.Settings),
            // "oauth2" => CreateOAuth2Authenticator(config.Settings),
            // "apiKey" => CreateApiKeyAuthenticator(config.Settings),
            // "customHeader" => CreateCustomHeaderAuthenticator(config.Settings),
            _ => TryFindImplementation(config.Type, config.Settings) ?? throw new ArgumentException(
                $"Unsupported authentication type: {config.Type}. Supported types are: bearer, basic, oauth2, apiKey, customHeader")
        };
    }

    private static IAuthenticator? TryFindImplementation(string configType, Dictionary<string, string> settings)
    {
        _availableAuthenticators.TryGetValue(configType, out var factory);
        if (factory != null)
            return factory?.Invoke(settings);
        return null;
    }


   

    public static List<AuthenticatorMetadata> AvailableAuthenticators()
    {
        return _availableAuthenticatorMetadata.Values.ToList();
    }
}
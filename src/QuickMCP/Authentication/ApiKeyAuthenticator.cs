using QuickMCP.Abstractions;
using QuickMCP.Types;

namespace QuickMCP.Authentication;

/// <summary>
/// Provides API key authentication (in header or query parameter)
/// </summary>
public class ApiKeyAuthenticator : IAuthenticator
{
    #region Fields and Properties

    private readonly string _apiKey;
    private readonly string _paramName;
    private readonly ApiKeyLocation _location;
    public string Type => Metadata.Type;
    public AuthenticatorMetadata Metadata => GetMetadata();

    public enum ApiKeyLocation
    {
        Header,
        QueryParameter
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Retrieves the metadata for the API key authenticator, including its name, description,
    /// configuration keys, and type.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticatorMetadata"/> containing details about the API key authenticator.</returns>
    public static AuthenticatorMetadata GetMetadata()
    {
        const string name = "API Key Authentication";
        const string description = $"API Key Authentication, it will add the API key to the header or query. ";

        List<(string Key, string Description, bool IsRequired)> configKeys =
        [
            ("apiKey", "The API Key used for authentication.", true),
            ("paramName", "The name of the parameter to hold the API key (default is X-API-Key).", false),
            ("location", "The location where the API key will be added (header or query).", false)
        ];
        const string type = "apiKey";
        return new AuthenticatorMetadata(name, description, configKeys, type);
    }

    /// <summary>
    /// Creates an API Key Authenticator.
    /// </summary>
    /// <param name="settings">The settings containing the API key, optional parameter name, and location.</param>
    /// <returns>An instance of <see cref="ApiKeyAuthenticator"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the 'apiKey' setting is missing.</exception>
    public static IAuthenticator Create(Dictionary<string, string?> settings)
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

        var location = ApiKeyLocation.Header;
        if (settings.TryGetValue("location", out var locationStr) &&
            locationStr != null &&
            locationStr.Equals("query", StringComparison.OrdinalIgnoreCase))
        {
            location = ApiKeyLocation.QueryParameter;
        }

        if(string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key authentication requires 'apiKey' setting");
        
        return new ApiKeyAuthenticator(apiKey!, paramName!, location);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Represents an API key authenticator that provides authentication via an API key,
    /// either by adding it to the header or as a query parameter in API requests.
    /// </summary>
    public ApiKeyAuthenticator(string apiKey, string paramName = "X-API-Key",
        ApiKeyLocation location = ApiKeyLocation.Header)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _paramName = paramName ?? throw new ArgumentNullException(nameof(paramName));
        _location = location;
    }

    #endregion

    #region IAuthenticator Implementation

    /// <inheritdoc/>
    public Task AuthenticateRequestAsync(HttpRequestMessage request)
    {
        if (_location == ApiKeyLocation.Header)
        {
            request.Headers.Add(_paramName, _apiKey);
        }
        else
        {
            var uri = request.RequestUri;
            if (uri != null)
            {
                var uriBuilder = new UriBuilder(uri);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query[_paramName] = _apiKey;
                uriBuilder.Query = query.ToString();
                request.RequestUri = uriBuilder.Uri;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, string>> GetAuthHeadersAsync()
    {
        if (_location == ApiKeyLocation.Header)
        {
            return Task.FromResult(new Dictionary<string, string>
            {
                [_paramName] = _apiKey
            });
        }

        return Task.FromResult(new Dictionary<string, string>());
    }

    /// <inheritdoc/>
    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_apiKey));
    }

    #endregion
}
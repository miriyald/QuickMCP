namespace QuickMCP.Types;

/// <summary>
/// A simple in-memory cache for storing OAuth tokens with expiration handling.
/// </summary>
public class OAuthCache
{
    private string? _token;
    private DateTimeOffset _expiry = DateTimeOffset.MinValue;

    /// <summary>
    /// Retrieves the cached token if it has not expired.
    /// </summary>
    /// <returns>The cached token, or <c>null</c> if no valid token is available.</returns>
    public string? GetToken()
    {
        if (_token != null && DateTimeOffset.UtcNow < _expiry)
        {
            return _token;
        }
        return null;
    }

    /// <summary>
    /// Stores the token in the cache with an expiration time.
    /// </summary>
    /// <param name="token">The token to be cached.</param>
    /// <param name="expiresIn">The number of seconds until the token expires. Defaults to 3600 seconds (1 hour).</param>
    public void SetToken(string token, int expiresIn = 3600)
    {
        _token = token;
        _expiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
    }
}
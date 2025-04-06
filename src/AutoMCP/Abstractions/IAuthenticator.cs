using AutoMCP.Types;

namespace AutoMCP.Abstractions;

/// <summary>
/// Interface for providing authentication to API requests
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// Gets the type of the authenticator.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the metadata containing the name, description, and configuration details for the authenticator.
    /// </summary>
    public AuthenticatorMetadata Metadata { get; }

    /// <summary>
    /// Authenticates an HTTP request by modifying its headers or other properties
    /// </summary>
    /// <param name="request">The HTTP request to authenticate</param>
    /// <returns>Task that completes when authentication is applied</returns>
    Task AuthenticateRequestAsync(HttpRequestMessage request);

    /// <summary>
    /// Gets authentication headers to be included in requests
    /// </summary>
    /// <returns>Dictionary of header names and values</returns>
    Task<Dictionary<string, string>> GetAuthHeadersAsync();

    /// <summary>
    /// Checks if the authenticator has valid credentials
    /// </summary>
    /// <returns>True if authentication is available and valid</returns>
    Task<bool> IsAuthenticatedAsync();
}
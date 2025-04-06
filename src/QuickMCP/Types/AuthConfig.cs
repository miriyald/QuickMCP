using System.Text.Json.Serialization;

namespace QuickMCP.Types;

/// <summary>
/// Represents the authentication configuration.
/// </summary>
public class AuthConfig
{
    /// <summary>
    /// Gets or sets the type of authentication.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication settings as a dictionary.
    /// </summary>
    [JsonPropertyName("settings")]
    public Dictionary<string, string?> Settings { get; set; } = new Dictionary<string, string?>();
}
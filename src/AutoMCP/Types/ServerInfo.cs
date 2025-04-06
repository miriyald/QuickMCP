using System.Text.Json.Serialization;

namespace AutoMCP.Types;

/// <summary>
/// Represents information about a server, including its name and description.
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// Gets or sets the name of the server.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the server.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
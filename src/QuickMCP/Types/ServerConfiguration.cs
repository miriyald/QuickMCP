namespace QuickMCP.Types;

public class ServerConfiguration
{
    /// <summary>
    /// Gets or sets the name of the server.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("serverName")]
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the description of the server.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the path to the configuration file.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("configurationFile")]
    public string? ConfigurationFile { get; set; }
}
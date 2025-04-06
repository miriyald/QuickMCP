using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace QuickMCP.Types;

/// <summary>
/// Represents a parameter used in API operations, including its name, location, type, and additional metadata.
/// </summary>
public class Parameter
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
        
    /// <summary>
    /// Gets or sets the location of the parameter (e.g., path, query, header, or body).
    /// </summary>
    [JsonPropertyName("in")]
    public string? In { get; set; }
        
    /// <summary>
    /// Gets or sets a value indicating whether the parameter is required.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; }
        
    /// <summary>
    /// Gets or sets the schema defining the type and structure of the parameter.
    /// </summary>
    [JsonPropertyName("schema")]
    public JsonNode? Schema { get; set; }
        
    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the content type of the parameter, if applicable.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}
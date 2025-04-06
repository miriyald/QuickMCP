using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace QuickMCP.Types;

/// <summary>
/// Represents information about an operation, including details such as summary, parameters, path, 
/// HTTP method, response schema, and tags.
/// </summary>
public class OperationInfo
{
    /// <summary>
    /// Gets or sets a brief description of the operation.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
        
    /// <summary>
    /// Gets or sets the list of parameters associated with this operation.
    /// </summary>
    [JsonPropertyName("parameters")]
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        
    /// <summary>
    /// Gets or sets the URI path of the operation.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
        
    /// <summary>
    /// Gets or sets the HTTP method to be used for the operation (e.g., GET, POST, etc.).
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
        
    /// <summary>
    /// Gets or sets the schema of the response returned by the operation, if applicable.
    /// </summary>
    [JsonPropertyName("responseSchema")]
    public JsonNode? ResponseSchema { get; set; }
        
    /// <summary>
    /// Gets or sets the tags categorizing the operation.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();
}
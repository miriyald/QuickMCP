using System.Text.Json.Nodes;

namespace AutoMCP.Models;

/// <summary>
/// Represents a resource containing a schema and associated metadata.
/// </summary>
public class ResourceInfo
{
    /// <summary>
    /// Gets or sets the schema for the resource in JSON format.
    /// </summary>
    public JsonNode Schema { get; set; } = null!;

    /// <summary>
    /// Gets or sets a dictionary containing metadata related to the resource.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
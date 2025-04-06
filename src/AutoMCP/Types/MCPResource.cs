using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AutoMCP.Types;

/// <summary>
/// Represents a managed configuration protocol (MCP) resource with metadata and schema information.
/// </summary>
public class McpResource
{
    /// <summary>
    /// Gets the name of the resource.
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; }

    /// <summary>
    /// Gets the JSON schema that defines the structure of the resource.
    /// </summary>
    [JsonPropertyName("schema")] public JsonNode Schema { get; }

    /// <summary>
    /// Gets the description of the resource.
    /// </summary>
    [JsonPropertyName("description")] public string Description { get; }

    /// <summary>
    /// Gets the URI for accessing the resource.
    /// </summary>
    [JsonPropertyName("uri")] public string Uri { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="schema">The JSON schema that defines the resource.</param>
    /// <param name="description">A brief description of the resource.</param>
    public McpResource(string name, JsonNode schema, string description)
    {
        Name = name;
        Schema = schema;
        Description = description;
        Uri = $"/resource/{name}";
    }
}
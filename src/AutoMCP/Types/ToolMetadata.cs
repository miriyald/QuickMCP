using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AutoMCP.Types;

/// <summary>
/// Represents metadata information for a tool, including its name, description, inputs, parameters, tags, server info, and response schema.
/// </summary>
public class ToolMetadata
{
    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input schema for the tool as a JSON node.
    /// </summary>
    [JsonPropertyName("inputSchema")]
    public JsonNode? InputSchema { get; set; }

    /// <summary>
    /// Gets or sets the list of parameters for the tool.
    /// </summary>
    [JsonPropertyName("parameters")]
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();

    /// <summary>
    /// Gets or sets the list of tags associated with the tool.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the server information for the tool.
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public ServerInfo? ServerInfo { get; set; }

    /// <summary>
    /// Gets or sets the response schema for the tool as a JSON node.
    /// </summary>
    [JsonPropertyName("responseSchema")]
    public JsonNode? ResponseSchema { get; set; }

    /// <summary>
    /// Converts the metadata properties to a dictionary representation.
    /// </summary>
    /// <returns>A dictionary containing metadata properties and their values.</returns>
    public Dictionary<string, object> ToDictionary()
    {
        var result = new Dictionary<string, object>
        {
            ["name"] = Name,
            ["description"] = Description
        };

        if (InputSchema != null)
        {
            result["inputSchema"] = InputSchema;
        }

        if (Parameters.Count > 0)
        {
            result["parameters"] = Parameters;
        }

        if (Tags.Count > 0)
        {
            result["tags"] = Tags;
        }

        if (ServerInfo != null)
        {
            result["serverInfo"] = ServerInfo;
        }

        if (ResponseSchema != null)
        {
            result["responseSchema"] = ResponseSchema;
        }

        return result;
    }
}
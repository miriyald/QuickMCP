using System.ComponentModel;

namespace QuickMCP.Types;

using System.Text.Json.Serialization;

public class MetadataUpdateConfig
{
    /// <summary>
    /// Gets or sets the list of tools to be updated.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<UpdatedToolMetadata> Tools { get; set; } = new();
}

public class UpdatedToolMetadata
{
    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    [JsonPropertyName("name")]
    [Description( "The name of the tool to be updated.")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the new name of the tool.
    /// </summary>
    [JsonPropertyName("newName")]
    [Description( "The new name of the tool.")]
    public string? NewName { get; set; }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    [JsonPropertyName("description")]
    [Description( "The description of the tool.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the prompt message for the tool.
    /// </summary>
    [JsonPropertyName("prompt")]
    [Description( "Agentic prompt template with placeholders for parameters in curly braces.")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the tool.
    /// </summary>
    [JsonPropertyName("parameters")]
    [Description( "The parameters for the tool.")]
    public List<UpdatedParameterMetadata>? Parameters { get; set; } 

    /// <summary>
    /// Gets or sets the tags associated with the tool.
    /// </summary>
    [JsonPropertyName("tags")]
    [Description( "The tags associated with the tool.")]
    public List<string> Tags { get; set; }
}

public class UpdatedParameterMetadata
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    [JsonPropertyName("name")]
    [Description( "The name of the parameter or body property name.")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    [JsonPropertyName("description")]
    [Description( "The description of the parameter.")]
    public string? Description { get; set; }
}
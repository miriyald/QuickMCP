using ModelContextProtocol.Protocol;
using System.Text.Json.Serialization;

namespace QuickMCP.Types;

/// <summary>
/// Represents an extended prompt containing additional arguments and messages.
/// </summary>
public class ExtendedPrompt
{

    /// <summary>
    /// Gets the name of the prompt.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets the description of the prompt.
    /// </summary>
    [JsonPropertyName("description")] 
    public string? Description { get; set; }

    /// <summary>
    /// Gets the collection of arguments for this prompt.
    /// </summary>
    [JsonPropertyName("arguments")] 
    public List<PromptArgument>? Arguments { get; set; }

    /// <summary>
    /// Gets the collection of messages for this prompt.
    /// </summary>
    [JsonPropertyName("messages")] 
    public List<PromptMessage>? Messages { get; set; }
}
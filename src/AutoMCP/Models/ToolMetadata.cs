using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AutoMCP.Models;

public class ToolMetadata
{
   
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
        
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
        
    [JsonPropertyName("inputSchema")]
    public JsonNode? InputSchema { get; set; }
        
    [JsonPropertyName("parameters")]
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();
        
    [JsonPropertyName("serverInfo")]
    public ServerInfo? ServerInfo { get; set; }
        
    [JsonPropertyName("responseSchema")]
    public JsonNode? ResponseSchema { get; set; }

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
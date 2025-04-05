using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AutoMCP.Models;

namespace AutoMCP.Helpers;

public class OperationInfo
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
        
    [JsonPropertyName("parameters")]
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
        
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
        
    [JsonPropertyName("responseSchema")]
    public JsonNode? ResponseSchema { get; set; }
        
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new List<string>();
}
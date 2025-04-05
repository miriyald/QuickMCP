using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace AutoMCP.Models;

public class Parameter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
        
    [JsonPropertyName("in")]
    public string? In { get; set; }
        
    [JsonPropertyName("required")]
    public bool Required { get; set; }
        
    [JsonPropertyName("schema")]
    public JsonNode? Schema { get; set; }
        
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }
}
using System.Text.Json.Nodes;

namespace AutoMCP.Models;

public class ResourceInfo
{
    public JsonNode Schema { get; set; } = null!;
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
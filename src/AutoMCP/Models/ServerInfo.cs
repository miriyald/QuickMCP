using System.Text.Json.Serialization;

namespace AutoMCP.Models;

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

}
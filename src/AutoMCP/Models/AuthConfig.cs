namespace AutoMCP.Models;

public class AuthConfig
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string>? Settings { get; set; }
}
namespace AutoMCP.Models;

public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    public string? ContentType { get; set; }
    public ToolMetadata Metadata { get; set; } = new ToolMetadata();
}
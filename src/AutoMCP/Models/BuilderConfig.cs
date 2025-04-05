namespace AutoMCP.Models;

/// <summary>
/// Configuration for builder
/// </summary>
public class BuilderConfig
{
    public string? ServerName { get; set; }
    public string? ServerDescription { get; set; }
    public string? ApiSpecUrl { get; set; }
    public string? ApiSpecPath { get; set; }
    public string? ServerBaseUri { get; set; }
    public AuthConfig? Authentication { get; set; }
    public bool GenerateResources { get; set; } = false;
    public bool GeneratePrompts { get; set; } = false;
}
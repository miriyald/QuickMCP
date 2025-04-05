using System.Text.Json;
using ModelContextProtocol.Protocol.Types;

namespace AutoMCP.Models;

public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    public string? MimeType { get; set; }
    public ToolMetadata Metadata { get; set; } = new ToolMetadata();

    public Tool ToProtocolTool()
    {
        return new Tool()
        {
            Name = Name,
            Description = Metadata.Description,
            Annotations = new ToolAnnotations()
            {
                OpenWorldHint = true,
                Title = Metadata.Description
            },
            InputSchema = Metadata.InputSchema is null ? new JsonElement() : JsonSerializer.Deserialize<JsonElement>(Metadata.InputSchema.ToJsonString(), AutoMcpJsonSerializerContext.Default.JsonElement)
        };
    }
}
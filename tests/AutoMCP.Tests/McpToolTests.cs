using System.Text.Json;
using AutoMCP.Builders;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using Shouldly;

namespace AutoMCP.Tests;

public class McpToolTests
{
    [Fact]
    public async Task ShouldCallMcpTool()
    {
        var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
            .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
        var tools = builder.GetMcpTools().ToList();
        
        var first = tools.FirstOrDefault(s=>s.ProtocolTool.Name.Contains("status", StringComparison.OrdinalIgnoreCase));

        
        var val = await first.InvokeAsync(new RequestContext<CallToolRequestParams>(null, new CallToolRequestParams()
        {
            Name = first.ProtocolTool.Name,
            Arguments = new Dictionary<string, JsonElement>()
            {
                ["status"] = JsonDocument.Parse("[\"sold\",\"available\",\"pending\"]").RootElement
            }
        } ));

        val.ShouldNotBeNull();
        
        val.Content.ShouldNotBeNull();
        val.Content[0].Text.ShouldContain("sold");

    }
}
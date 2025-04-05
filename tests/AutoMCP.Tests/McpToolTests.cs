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
    public async Task ShouldCallMcpTool_Get_WithQuery()
    {
        var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
            .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
        var tools = builder.GetMcpTools().ToList();
        
        //Query test
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

    [Fact]
    public async Task ShouldCallMcpTool_Get_WithPathParams()
    {
        var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
            .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
        var tools = builder.GetMcpTools().ToList();

        //Get with path params
        var second = tools.FirstOrDefault(s=>s.ProtocolTool.Name.Contains("getPetById", StringComparison.OrdinalIgnoreCase));
        
        var secondResult = await second.InvokeAsync(new RequestContext<CallToolRequestParams>(null, new CallToolRequestParams()
        {
            Name = second.ProtocolTool.Name,
            Arguments = new Dictionary<string, JsonElement>()
            {
                ["petId"] =  JsonDocument.Parse("100").RootElement
            }
        } ));
        
        secondResult.Content.ShouldNotBeNull();
        secondResult.Content[0].Text.ShouldContain("status");
    }

    [Fact]
    public async Task ShouldCallMcpTool_Post_WithBody()
    {
        var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
            .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
        var tools = builder.GetMcpTools().ToList();
        
        //Post
        var third = tools.FirstOrDefault(s=>s.ProtocolTool.Name.Contains("addPet", StringComparison.OrdinalIgnoreCase));
        
        var thirdResult = await third.InvokeAsync(new RequestContext<CallToolRequestParams>(null, new CallToolRequestParams()
        {
            Name = third.ProtocolTool.Name,
            Arguments = new Dictionary<string, JsonElement>()
            {
                ["body"] =  JsonDocument.Parse("{\n    \"id\": 12345,\n    \"category\": {\n      \"id\": 1,\n      \"name\": \"Dogs\"\n    },\n    \"name\": \"Buddy\",\n    \"photoUrls\": [\n      \"http://example.com/buddy1.jpg\",\n      \"http://example.com/buddy2.jpg\"\n    ],\n    \"tags\": [\n      {\n        \"id\": 10,\n        \"name\": \"friendly\"\n      },\n      {\n        \"id\": 11,\n        \"name\": \"playful\"\n      }\n    ],\n    \"status\": \"available\"\n }").RootElement
            }
        } ));
        
        thirdResult.Content.ShouldNotBeNull();
        thirdResult.Content[0].Text.ShouldContain("Buddy");
    }
    
    [Fact]
    public async Task ShouldCallMcpTool_Put_WithBody()
    {
        var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
            .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
        var tools = builder.GetMcpTools().ToList();
        
        //Put
        var third = tools.FirstOrDefault(s=>s.ProtocolTool.Name.Contains("updatePet", StringComparison.OrdinalIgnoreCase));
        
        var thirdResult = await third.InvokeAsync(new RequestContext<CallToolRequestParams>(null, new CallToolRequestParams()
        {
            Name = third.ProtocolTool.Name,
            Arguments = new Dictionary<string, JsonElement>()
            {
                ["petId"] =  JsonDocument.Parse("9876").RootElement,
                ["body"] =  JsonDocument.Parse("{\n    \"name\": \"Sparky\",\n    \"status\": \"sold\"\n  }").RootElement
            }
        } ));
        
        thirdResult.Content.ShouldNotBeNull();
        thirdResult.Content[0].Text.ShouldContain("Sparky");
    }
    
    // [Fact]
    // public async Task ShouldCallMcpTool_Delete_WithBody()
    // {
    //     var builder = await McpServerInfoBuilder.ForOpenApi("Test_Server")
    //         .FromUrl("https://petstore.swagger.io/v2/swagger.json").OnlyForPaths(["pet"]).BuildAsync();
    //     var tools = builder.GetMcpTools().ToList();
    //     
    //     //Put
    //     var third = tools.FirstOrDefault(s=>s.ProtocolTool.Name.Contains("deletePet", StringComparison.OrdinalIgnoreCase));
    //     
    //     var thirdResult = await third.InvokeAsync(new RequestContext<CallToolRequestParams>(null, new CallToolRequestParams()
    //     {
    //         Name = third.ProtocolTool.Name,
    //         Arguments = new Dictionary<string, JsonElement>()
    //         {
    //             ["petId"] =  JsonDocument.Parse("9876").RootElement,
    //         }
    //     } ));
    //     
    //     thirdResult.Content.ShouldNotBeNull();
    //     thirdResult.Content[0].Text.ShouldContain("Sparky");
    // }
}
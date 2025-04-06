using AutoMCP.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMCP.Extensions;

public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithAutoMcp(this IMcpServerBuilder builder, McpServerInfo mcpServerInfo)
    {
        foreach (var tool in mcpServerInfo.GetMcpTools())
        {
            builder.Services.AddSingleton(tool);
        } 

        return builder;
    }
}
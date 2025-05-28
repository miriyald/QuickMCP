using QuickMCP.Types;
using Microsoft.Extensions.DependencyInjection;

namespace QuickMCP.Extensions;

public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithQuickMCP(this IMcpServerBuilder builder, McpServerInfo mcpServerInfo)
    {
        foreach (var tool in mcpServerInfo.GetMcpTools())
        {
            builder.Services.AddSingleton(tool);
        }

        return builder;
    }
}
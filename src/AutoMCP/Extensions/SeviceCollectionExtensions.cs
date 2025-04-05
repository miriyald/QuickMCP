using AutoMCP.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMCP.Extensions;

public static class SeviceCollectionExtensions
{
    public static void AddAutoMCP(this IServiceCollection services, McpServerInfo mcpServerInfo)
    {
        foreach (var tool in mcpServerInfo.GetMcpTools())
        {
            services.AddSingleton(tool);
        }
    }
}
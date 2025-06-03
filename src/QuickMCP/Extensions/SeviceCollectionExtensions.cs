using QuickMCP.Types;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using QuickMCP.Server;

namespace QuickMCP.Extensions;

public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithQuickMCP(this IMcpServerBuilder builder, McpServerInfo mcpServerInfo)
    {
        foreach (var tool in mcpServerInfo.GetMcpTools())
        {
            builder.Services.AddSingleton(tool);
        }

        var haveResources = false;
        if (mcpServerInfo.Resources.Count > 0)
        {
            ResourceRegistry.ApiResources = mcpServerInfo.Resources;
            builder = builder.WithResources<ResourceRegistry>()
                .WithListResourcesHandler(ResourceRegistry.GetList);
            haveResources = true;
        }

        if (mcpServerInfo.BuilderConfig.ExternalResources?.Count > 0)
        {
            if (!string.IsNullOrEmpty(mcpServerInfo.BuilderConfig.ExternalResourcesRoot))
                ResourceRegistry.Root = mcpServerInfo.BuilderConfig.ExternalResourcesRoot;
            
            ResourceRegistry.ExternalResources =
                mcpServerInfo.BuilderConfig.ExternalResources?.ToDictionary(r => r.Name, r => r) ??
                new Dictionary<string, Resource>();
            haveResources = true;
        }

        if (haveResources)
        {
            builder = builder.WithResources<ResourceRegistry>()
                .WithListResourcesHandler(ResourceRegistry.GetList);
        }

        var havePrompts = mcpServerInfo.Prompts.Count > 0 || mcpServerInfo.BuilderConfig.ExternalResources?.Count > 0;
        if (havePrompts)
        {
            PromptsRegistry.ApiPrompts = mcpServerInfo.Prompts;
            PromptsRegistry.ExtendedPrompts =
                mcpServerInfo.BuilderConfig.ExtendedPrompts?.ToDictionary(p => p.Name, p => p) ??
                new Dictionary<string, ExtendedPrompt>();
            builder = builder.WithListPromptsHandler(PromptsRegistry.GetList)
                .WithGetPromptHandler(PromptsRegistry.GetPrompt);
        }

        return builder;
    }
}
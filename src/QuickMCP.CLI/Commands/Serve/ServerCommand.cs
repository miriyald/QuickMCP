using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using QuickMCP.Abstractions;
using QuickMCP.Builders;
using QuickMCP.Extensions;
using QuickMCP.Types;
using Spectre.Console;
using Spectre.Console.Cli;
using Vertical.SpectreLogger;

namespace QuickMCP.CLI.Commands.Serve;


[Description("Starts the MCP server with the provided configuration or arguments.")]
public class ServerCommand : AsyncCommand<ServerCommandSettings>
{

    public override async Task<int> ExecuteAsync(CommandContext context, ServerCommandSettings settings)
    {
        var configFile = settings.ConfigPath;
        var configEnvVar = settings.ConfigEnvVar;
        if (!string.IsNullOrWhiteSpace(configEnvVar))
        {
            configFile = Environment.GetEnvironmentVariable(configEnvVar);
        }

        if (!string.IsNullOrWhiteSpace(settings.ServerId))
        {
            var localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".quickmcp");
            var serverFolder = Path.Combine(localFolder, "servers", settings.ServerId);
            configFile = Path.Combine(serverFolder, "config.json");

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException($"Server configuration file not found for {settings.ServerId} at `{configFile}`");
            }
        }

        if (settings.EnableLogging == true)
        {
            AnsiConsole.MarkupLine($"[bold]Building MCP server info with configuration file:[/] {configFile}");
        }

        IMcpServerInfoBuilder? infoBuilder = null;

        if (!string.IsNullOrWhiteSpace(configFile))
        {
            infoBuilder = McpServerInfoBuilder.FromConfig(configFile);
        }
        else
        {
            var config = settings.GetBuilderConfig();
            infoBuilder = McpServerInfoBuilder.FromConfig(config);
        }

        if (settings.EnableLogging == true)
        {
            var loggerFactory = LoggerFactory.Create(s => s.AddSpectreConsole());
            infoBuilder.AddLogging(loggerFactory);
        }

        var mcpServerInfo = await infoBuilder.BuildAsync();

        if (settings.EnableLogging == true)
        {
            AnsiConsole.MarkupLine($"[bold]Initializing MCP server...[/]");
        }

        var logLevel = settings.EnableLogging == true ? LogLevel.Debug : LogLevel.None;

        var protocol = (settings.HostProtocol  ??  mcpServerInfo.BuilderConfig.HostProtocol)?.ToLower();
        if (protocol == "http")
        {
            await RunHttpServerAsync(mcpServerInfo, configFile, logLevel, settings.EnableLogging == true);
        }
        else
        {
            await RunStdioServerAsync(mcpServerInfo, configFile, logLevel, settings.EnableLogging == true);
        }

        return 0;
    }
    public async Task RunHttpServerAsync(McpServerInfo mcpServerInfo,
                                            string? configFile,
                                            LogLevel logLevel,
                                            bool enableLogging)
    {
        var hostBuilder = WebApplication.CreateBuilder();
        var mcpBuilder = hostBuilder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithQuickMCP(mcpServerInfo);

      
        hostBuilder.Logging.SetMinimumLevel(logLevel).AddSpectreConsole(config =>
        {
            config.SetMinimumLevel(logLevel);
        });

        var app = hostBuilder.Build();
        app.MapMcp();

        if (enableLogging)
        {
            AnsiConsole.MarkupLine($"[bold]MCP server started...[/]");
        }
        await app.RunAsync();
    }

    public async Task RunStdioServerAsync(McpServerInfo mcpServerInfo,
                                            string? configFile,
                                            LogLevel logLevel,
                                            bool enableLogging)
    {
        if (enableLogging)
        {
            AnsiConsole.MarkupLine($"[bold]Using STDIO transport...[/]");
        }
        var hostBuilder = Host.CreateApplicationBuilder();
        var mcpBuilder = hostBuilder.Services
            .AddMcpServer()
            .WithQuickMCP(mcpServerInfo)
            .WithStdioServerTransport();

        // var haveResources = false;
        // if (mcpServerInfo.Resources.Count > 0)
        // {
        //     ResourceRegistry.ApiResources = mcpServerInfo.Resources;
        //     mcpBuilder = mcpBuilder.WithResources<ResourceRegistry>()
        //                             .WithListResourcesHandler(ResourceRegistry.GetList);
        //     haveResources = true;
        // }
        //
        // if (mcpServerInfo.BuilderConfig.ExternalResources?.Count > 0)
        // {
        //     ResourceRegistry.Root = Path.GetDirectoryName(configFile);
        //     ResourceRegistry.ExternalResources = mcpServerInfo.BuilderConfig.ExternalResources?.ToDictionary(r => r.Name, r => r) ?? new Dictionary<string, Resource>();
        //     haveResources = true;
        // }
        //
        // if (haveResources)
        // {
        //     mcpBuilder = mcpBuilder.WithResources<ResourceRegistry>()
        //                            .WithListResourcesHandler(ResourceRegistry.GetList);
        // }
        //
        // var havePrompts = mcpServerInfo.Prompts.Count > 0 || mcpServerInfo.BuilderConfig.ExternalResources?.Count > 0;
        // if (havePrompts)
        // {
        //     PromptsRegistry.ApiPrompts = mcpServerInfo.Prompts;
        //     PromptsRegistry.ExtendedPrompts = mcpServerInfo.BuilderConfig.ExtendedPrompts?.ToDictionary(p => p.Name, p => p) ?? new Dictionary<string, ExtendedPrompt>();
        //     mcpBuilder = mcpBuilder.WithListPromptsHandler(PromptsRegistry.GetList)
        //                            .WithGetPromptHandler(PromptsRegistry.GetPrompt);
        //
        // }

        hostBuilder.Logging.SetMinimumLevel(logLevel).AddSpectreConsole(config =>
        {
            config.SetMinimumLevel(logLevel);
        });

        var app = hostBuilder.Build();
        if (enableLogging)
        {
            AnsiConsole.MarkupLine($"[bold]MCP server started...[/]");
        }
        await app.RunAsync();
    }

}
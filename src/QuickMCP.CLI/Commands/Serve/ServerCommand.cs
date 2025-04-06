using System.ComponentModel;
using QuickMCP.Abstractions;
using QuickMCP.Builders;
using QuickMCP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            configFile = Environment.GetEnvironmentVariable(configEnvVar);

        if (settings.EnableLogging == true)
            AnsiConsole.MarkupLine($"[bold]Building MCP server info with configuration file:[/] {configFile}");

        IMcpServerInfoBuilder? infoBuilder = null;

        if (!string.IsNullOrWhiteSpace(configFile))
            infoBuilder = McpServerInfoBuilder.FromConfig(configFile);
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
        else
        {
            
        }

        var mcpServerInfo = await infoBuilder.BuildAsync();
        if (settings.EnableLogging == true)
            AnsiConsole.MarkupLine($"[bold]Initializing MCP server...[/]");
        
        var hostBuilder = Host.CreateApplicationBuilder();
        var mcpBuilder = hostBuilder.Services
            .AddMcpServer()
            .WithQuickMCP(mcpServerInfo)
            .WithStdioServerTransport();

        // if (mcpServerInfo.PromptCount == 0)
        // {
        //     mcpBuilder.WithListPromptsHandler((s, b) =>
        //     {
        //         return Task.FromResult(new ListPromptsResult());
        //     });
        //     mcpBuilder.WithGetPromptHandler( (s, b) =>
        //     {
        //         return Task.FromResult(new GetPromptResult());
        //     });
        // }
        //
        // if (mcpServerInfo.ResourceCount == 0)
        // {
        //     mcpBuilder.WithListResourcesHandler((a, b) => Task.FromResult(new ListResourcesResult()));
        // }

        if (settings.EnableLogging == true)
        {
            var logLevel = settings.EnableLogging == true ? LogLevel.Debug : LogLevel.None;

            hostBuilder.Logging.SetMinimumLevel(logLevel).AddSpectreConsole(config =>
            {
                config.SetMinimumLevel(logLevel);
            });
        }

        if (settings.EnableLogging == true)
            AnsiConsole.MarkupLine($"[bold]Starting MCP server...[/]");
        //AnsiConsole.MarkupLine("[green]MCP Server Started (Press Ctrl+C to exit)[/]");
        await hostBuilder.Build().RunAsync();
        return 0;
    }
}
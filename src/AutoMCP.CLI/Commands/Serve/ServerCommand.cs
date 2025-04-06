using System.ComponentModel;
using AutoMCP.Abstractions;
using AutoMCP.Builders;
using AutoMCP.CLI.Settings;
using AutoMCP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using Spectre.Console;
using Spectre.Console.Cli;
using Vertical.SpectreLogger;

namespace AutoMCP.CLI.Commands;


[Description("Starts the MCP server with the provided configuration or arguments.")]
public class ServerCommand:AsyncCommand<ServerCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ServerCommandSettings settings)
    {
        var configFile = settings.ConfigPath;
        var configEnvVar = settings.ConfigEnvVar;
        if(!string.IsNullOrWhiteSpace(configEnvVar))
            configFile = Environment.GetEnvironmentVariable(configEnvVar);

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        //AnsiConsole.MarkupLine($"[bold]Building MCP server info with configuration file:[/] {configFile}");
       
        IMcpServerInfoBuilder? infoBuilder = null;
        
        if(!string.IsNullOrWhiteSpace(configFile))
            infoBuilder = McpServerInfoBuilder.FromConfig(configFile);
        else
        {
            var config = settings.GetBuilderConfig();
            infoBuilder = McpServerInfoBuilder.FromConfig(config);
        }

        //infoBuilder.AddLogging(loggerFactory);
        var mcpServerInfo = await infoBuilder.BuildAsync();
        
        //AnsiConsole.MarkupLine($"[bold]Initializing MCP server...[/]");
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.Services
            .AddMcpServer()
            .WithAutoMcp(mcpServerInfo)
            .WithStdioServerTransport();
            // .WithListPromptsHandler(async (blah, ct) =>
            // {
            //     return new ListPromptsResult();
            // })
            // .WithGetPromptHandler(async (blah,b) =>
            // {
            //     return new GetPromptResult();
            // });
            

        hostBuilder.Logging.SetMinimumLevel(LogLevel.None).AddSpectreConsole(config =>
        {
            config.SetMinimumLevel(LogLevel.None);
        });
        
        //AnsiConsole.MarkupLine($"[bold]Starting MCP server...[/]");
        await hostBuilder.Build().RunAsync();
        return 0;
    }
}
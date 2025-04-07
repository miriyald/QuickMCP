using QuickMCP.CLI.Commands;
using QuickMCP.CLI.Commands.Build;
using QuickMCP.CLI.Commands.List;
using QuickMCP.CLI.Commands.Serve;
using Spectre.Console.Cli;

var commandApp = new CommandApp();

commandApp.Configure(app =>
{
    app.AddCommand<ServerCommand>("serve").WithAlias("server").WithExample("serve","-c mcp_server_config.json");

    app.AddBranch<BuildCommandSettings>("build", build =>
    {
        build.SetDescription("Build a MCP server configuration file and more");
        build.AddCommand<BuildConfigCommand>("config").WithAlias("config-file").WithAlias("config-file");
    });
    
    app.AddBranch<ListCommandSettings>("list", list =>
    {
        list.SetDescription("List available configuration options such as authenticators");
        list.AddCommand<ListAuthenticatorsCommand>("auth").WithAlias("authenticator").WithAlias("authenticators");
    });
});
return await commandApp.RunAsync(args).ConfigureAwait(false);

using AutoMCP.CLI.Commands;
using AutoMCP.CLI.Commands.List;
using Spectre.Console.Cli;

var commandApp = new CommandApp();

commandApp.Configure(app =>
{
    app.AddCommand<ServerCommand>("serve").WithAlias("server").WithExample("ser");
    app.AddBranch<ListCommandSettings>("list", list =>
    {
        list.SetDescription("List available configuration options such as authenticators");
        list.AddCommand<ListAuthenticatorsCommand>("auth").WithAlias("authenticator").WithAlias("authenticators");
    });
});
return await commandApp.RunAsync(args).ConfigureAwait(false);

// var rootCommand = new RootCommand(
//     description: "AutoMCP CLI: build and run Modal Context Protocol (MCP) servers with ease.");
// rootCommand.AddCommand(new ServerCommand());
// rootCommand.AddCommand(new ListAuthenticatorsCommand());
//
// return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
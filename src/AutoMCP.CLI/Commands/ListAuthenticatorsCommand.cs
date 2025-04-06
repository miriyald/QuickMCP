
using System.ComponentModel;
using AutoMCP.Authentication;
using AutoMCP.CLI.Commands.List;
using Spectre.Console;
using Spectre.Console.Cli;
using Color = System.Drawing.Color;

namespace AutoMCP.CLI.Commands;

[Description("List available configuration options such as authenticators")]
public class ListAuthenticatorsCommand : Command<ListAuthenticatorsCommandSettings>
{
    // public ListAuthenticatorsCommand() : base("list-auth",
    //     description: "List available configuration options such as authenticators")
    // {
    //     this.SetHandler(HandleAsync);
    // }

   
    public override int Execute(CommandContext context, ListAuthenticatorsCommandSettings settings)
    {
        var auths = AuthenticatorFactory.AvailableAuthenticators();
        AnsiConsole.WriteLine();
        foreach (var auth in auths)
        {
            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow($"[bold]Name[/]: [cyan]{auth.Name}[/]");
            grid.AddRow($"[bold]Description[/]: [cyan]{auth.Description}[/]");
            grid.AddRow($"[bold]Type[/]: [cyan]{auth.Type}[/]");
            var keysTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("[yellow]Key[/]")
                .AddColumn("[yellow]Description[/]")
                .AddColumn("[yellow]Required[/]");
            if (auth.ConfigKeys.Any())
            {
                foreach (var key in auth.ConfigKeys)
                {
                    keysTable.AddRow(
                        $"[green]{key.Key}[/]",
                        $"[cyan]{key.Description}[/]",
                        key.IsRequired ? "[bold red]Yes[/]" : "[bold green]No[/]"
                    );
                }
                grid.AddRow();
                grid.AddRow("[bold]Auth Configuration Settings Keys:[/]");
                grid.AddRow(keysTable);
            }

            var panel = grid;

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
        
        AnsiConsole.MarkupLine($"Available authenticator types: [cyan]{string.Join(", " ,auths.Select(s=>s.Type))} [/]");
        return 0;
    }
}
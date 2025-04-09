using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.List;

public class ListServerCommandSettings:ListCommandSettings
{
    [CommandOption("-f|--filter <FILTER>")]
    public string? Filter { get; set; }
}
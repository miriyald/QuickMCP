using System.ComponentModel;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.List;


public class ListAuthenticatorsCommandSettings : ListCommandSettings
{
    [Description("Authenticator type to list")]
    [CommandArgument(0, "[AUTHENTICATOR]")]
    public string? Authenticator { get; set; }
}
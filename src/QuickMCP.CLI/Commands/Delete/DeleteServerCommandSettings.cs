using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.Delete;

public class DeleteServerCommandSettings:DeleteCommandSettings
{
    [Description("Server name or Id")]
    [CommandArgument(0, "[server name]")]
    public string? ServerName { get; set; }

    public override ValidationResult Validate()
    {
        if(string.IsNullOrWhiteSpace(ServerName))
            return ValidationResult.Error("Server name or Id is required");
        
        return base.Validate();
    }
}
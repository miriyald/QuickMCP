using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.Add;

public class AddServerSettings : AddCommandSettings
{
    [Description("Path to the configuration file")]
    [CommandArgument(0, "[configuration file]")]
    public string? ConfigurationFile { get; set; }

    [Description("Name of the server")]
    [CommandOption("-n|--name <NAME>")]
    public string? ServerName { get; set; }

    public override ValidationResult Validate()
    {
        if(string.IsNullOrWhiteSpace(ConfigurationFile))
            return ValidationResult.Error("Configuration file is required");
        return base.Validate();
    }
}
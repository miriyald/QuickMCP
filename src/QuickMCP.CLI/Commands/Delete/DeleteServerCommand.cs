using System.ComponentModel;
using QuickMCP.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QuickMCP.CLI.Commands.Delete;

[Description("Delete a stored server")]
public class DeleteServerCommand:AsyncCommand<DeleteServerCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeleteServerCommandSettings settings)
    {
        var serverName = settings.ServerName;
        var serverId = StringHelpers.SanitizeServerName(serverName);
        var localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".quickmcp");
        var serverFolder = Path.Combine(localFolder, "servers", serverId);

        if (Directory.Exists(serverFolder))
        {
            if (await AnsiConsole.ConfirmAsync($"Are you sure you want to delete the server [bold]{serverName}[/]?"))
            {
                Directory.Delete(serverFolder, true);
                AnsiConsole.MarkupLine($"[green]Server [bold]{serverName}[/] deleted[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Deletion of server [bold]{serverName}[/] cancelled[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Server [bold]{serverName}[/] not found[/]");
        }

        return 0;
    }
}
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace FurmAppDBot.Commands;

// TODO: Form container commands.
public class ContainerCommandsModule : BaseCommandModule
{
    [Command(CMD_CONSTANT.CHANNEL_CONTAINER_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Container(CommandContext ctx)
    {
        
    }
}
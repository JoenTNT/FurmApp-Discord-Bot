using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.PREFIX_COMMAND_NAME, CMD_CONSTANT.PREFIX_COMMAND_DESCRIPTION)]
public class PrefixSlashCommandGroup : ApplicationCommandModule
{
    // TODO: Prefix Command.
    // [SlashCommand(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME, CMD_CONSTANT.GET_PREFIX_COMMAND_DESCRIPTION)]
    // public async Task GetPrefix(InteractionContext ctx)
    //     => await PrefixCommand.GetPrefix(ctx);
}
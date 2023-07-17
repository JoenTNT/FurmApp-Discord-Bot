using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands;

public class SlashCommandFunctions : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.PING_COMMAND_NAME, CMD_CONSTANT.PING_COMMAND_DESCRIPTION)]
    public async Task Ping(InteractionContext ctx)
        => await PingCommand.Ping(ctx);

    // TODO: Prefix Command.
    // [SlashCommand(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME, CMD_CONSTANT.GET_PREFIX_COMMAND_DESCRIPTION)]
    // public async Task GetPrefix(InteractionContext ctx)
    //     => await PrefixCommand.GetPrefix(ctx);

    [SlashCommand(CMD_CONSTANT.PURGE_COMMAND_NAME, CMD_CONSTANT.PURGE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Purge(InteractionContext ctx,
        [Option(CMD_CONSTANT.AMOUNT_PARAMETER, CMD_CONSTANT.PURGE_AMOUNT_PARAMETER_DESCRIPTION, true)]
        string purgeAmount)
        => await PurgeCommand.Purge(ctx, purgeAmount);
}
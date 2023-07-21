using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands.Attributes;
using FurmAppDBot.Databases;

namespace FurmAppDBot.Commands;

public class CommandNextFunctions : BaseCommandModule
{
    [Command(CMD_CONSTANT.PING_COMMAND_NAME)]
    public async Task Ping(CommandContext ctx) => await PingCommand.Ping(ctx);

    // TODO: Prefix Command.
    // [Command(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME)]
    // public async Task GetPrefix(CommandContext ctx)
    //     => await PrefixCommand.GetPrefix(ctx);

    [Command(CMD_CONSTANT.PURGE_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Purge(CommandContext ctx, string purgeAmount)
        => await PurgeCommand.Purge(ctx, purgeAmount);

    // [Command(CMD_CONSTANT.SYNCDB_COMMAND_NAME)]
    // [DeveloperOnly]
    // public async Task SyncDB(CommandContext ctx)
    // {
    //     // Clearing unused data from database.
    //     var db = MainDatabase.Instance;
    //     await ctx.Message.DeleteAsync();
    //     var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
    //     foreach (var guild in ctx.Client.Guilds)
    //     {
    //         FurmAppClient.Instance.Logger.LogInformation($"Cleaning up server (ID: {guild.Key})");
    //         await db.DeleteAllUnusedMissingMessage(guild.Value);
    //     }
    //     await msgHandler.ModifyAsync("All Done!");
    // }
}
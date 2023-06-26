using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;

namespace FurmAppDBot.Commands;

public class CommandNextFunctions : BaseCommandModule
{
    [Command(CMD_CONSTANT.PING_COMMAND_NAME)]
    public async Task Ping(CommandContext ctx) => await PingCommand.Ping(ctx);

    [Command(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME)]
    public async Task GetPrefix(CommandContext ctx) => await PrefixCommand.GetPrefix(ctx);

    [Command(CMD_CONSTANT.PURGE_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Purge(CommandContext ctx, string purgeAmount) => await PurgeCommand.Purge(ctx, purgeAmount);

    [Command(CMD_CONSTANT.SET_BUTTON_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task SetButton(CommandContext ctx, string messageID) => await ButtonCommand.SetButton(ctx, messageID);

    [Command(CMD_CONSTANT.HELP_COMMAND_NAME)]
    public async Task Help(CommandContext ctx) => await HelpCommand.Help(ctx);

    [Command("syncdb")]
    [DeveloperOnly]
    public async Task SyncDB(CommandContext ctx)
    {
        var db = MainDatabase.Instance;
        await ctx.Message.DeleteAsync();

        var messageResponse = new DiscordMessageBuilder()
            .WithContent("Please wait for a moment...");

        var processInfoMessage = await ctx.Channel.SendMessageAsync(messageResponse);

        foreach (var guild in ctx.Client.Guilds)
        {
            FurmAppClient.Instance.Logger.LogInformation($"Cleaning up server (ID: {guild.Key})");
            await db.DeleteAllUnusedMissingMessage(guild.Value);
        }

        messageResponse = new DiscordMessageBuilder()
            .WithContent("All Done!");
        
        await processInfoMessage.ModifyAsync(messageResponse);
    }
}
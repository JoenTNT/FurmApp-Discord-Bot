using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public class PingCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.PING_COMMAND_NAME)]
    public async Task Ping(CommandContext ctx)
    {
        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        try
        {
            // Run ping command.
            await PingCommandsModule.Ping(msgHandler, ctx.Client);
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    #endregion

    #region Statics

    public static async Task Ping(DiscordMessage msgHandler, DiscordClient client)
    {
        // Create fancy embed.
        var embed = new DiscordEmbedBuilder()
        {
            Description = $"Pong! {client.Ping}ms",
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
        };

        // Notify latency value.
        await msgHandler.ModifyAsync(new DiscordMessageBuilder { Embed = embed, });
    }

    #endregion

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
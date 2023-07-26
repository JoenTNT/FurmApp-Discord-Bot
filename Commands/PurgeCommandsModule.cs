using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace FurmAppDBot.Commands;

public class PurgeCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.PURGE_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Purge(CommandContext ctx, string purgeAmount)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Start purging process.
                await Purge(msgHandler, ctx.Channel, int.Parse(purgeAmount), true);
            }
            catch (NotFoundException) // When purging is interrupted.
            {
                // Notify message handler.
                await msgHandler.ModifyAsync("Purging has been interrupted, abort the process.");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Purge amount must be number, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
    
    #endregion

    #region Statics

    public static async Task Purge(DiscordMessage msgHandler, DiscordChannel channel, int purgeAmount, bool extraMsgByCmdNext = false)
    {
        // Get all messages in channel, including the message handler.
        var messages = await channel.GetMessagesAsync(purgeAmount + 1 + (extraMsgByCmdNext ? 1 : 0));
        int amount = messages.Count - 1 - (extraMsgByCmdNext ? 1 : 0);
        var initialMessage = await msgHandler.ModifyAsync("Purging in progress, please wait and do not delete messages manually...");
        
        // Purging process, skip the message handler that recently sent.
        for (int i = 1; i < messages.Count; i++)
            await messages[i].DeleteAsync();

        // Notify purging finished, then automatically delete message handler.
        await initialMessage.ModifyAsync(new DiscordMessageBuilder()
            .WithContent($"Purge successfully done! (Purged {amount} messages)\n"
            + "This message will automatically deleted in 3 seconds... 3 2 1 ..."));
        await Task.Delay(3000); // Wait for 3 seconds.
        await initialMessage.DeleteAsync();
    }

    #endregion
}
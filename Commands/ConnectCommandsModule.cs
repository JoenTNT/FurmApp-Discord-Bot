using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Extensions;

namespace FurmAppDBot.Commands;

public class ConnectCommandsModule: BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.CONNECT_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Connect(CommandContext ctx, string messageID, string buttonID, string formID)
    {
        // Initial respond with message handler.
        var msgHandler = await ctx.Message.RespondAsync("Please wait for a moment...");
        
        try
        {
            try
            {
                // Search for target message.
                DiscordMessage msgFound = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));

                // Check if the target message has the button with specific ID, if not then abort.
                if (!msgFound.IsComponentWithIDExists(buttonID))
                {
                    await msgHandler.ModifyAsync($"Button with ID `{buttonID}` not found, abort the process.\n"
                        + "This message will be delete automatically in 3 2 1...");
                    await Task.Delay(3000);
                    await msgHandler.DeleteAsync();
                    return;
                }

                // Set button process
                await Connect(msgHandler, msgFound, buttonID, formID);
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Bad argument inserted to message ID, insert numbers only, abort the process.");
            }
            catch (NotFoundException) // When message not found.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process.");
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException) // When getting form data failed.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    #endregion
    
    #region Static

    public static async Task Connect(DiscordMessage msgHandler, DiscordMessage targetMsg, string buttonID, string formID)
    {
        // Get button and form information.
        var btnData = await InterfaceData.GetData(targetMsg.Channel.Guild.Id, targetMsg.Channel.Id);
        var formData = await FormData.GetData(targetMsg.Channel.Guild.Id, formID);

        // Finally connect both data.
        await btnData.ConnectElement(formData, $"{targetMsg.Id}", buttonID);

        // Final conclusion.
        await msgHandler.ModifyAsync($"Button component `{buttonID}` has been connected with Form `{formData.FormID}`\n"
            + "You can now delete this message.\n\n"
            + $"Kindly double check the modified message: https://discord.com/channels/{targetMsg.Channel.GuildId}/{targetMsg.ChannelId}/{targetMsg.Id}");
    }

    #endregion
}
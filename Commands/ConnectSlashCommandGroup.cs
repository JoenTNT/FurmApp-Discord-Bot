using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Extensions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.CONNECT_COMMAND_NAME, CMD_CONSTANT.CONNECT_COMMAND_DESCRIPTION)]
public class ConnectSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.BUTTON_COMMAND_NAME, CMD_CONSTANT.BUTTON_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task ButtonForm(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION)]
        string messageID,
        [Option(CMD_CONSTANT.BUTTON_ID_PARAMETER, CMD_CONSTANT.BUTTON_ID_PARAMETER_DESCRIPTION)]
        string buttonID,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION)]
        string formID)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

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
                await rmHandler.DeleteAsync();
                return;
            }

            // Set button process
            await ConnectCommandsModule.Connect(msgHandler, msgFound, buttonID, formID);
            await rmHandler.DeleteAsync();
        }
        catch (FormatException) // Wrong input format.
        {
            try
            {
                await rmHandler.DeleteAsync();
                await msgHandler.ModifyAsync("Bad argument inserted to message ID, insert numbers only.\n"
                    + "This message will be delete automatically in 3 2 1...");
                await Task.Delay(3000);
                await msgHandler.DeleteAsync();
            }
            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        }
        catch (NotFoundException) // When message not found.
        {
            try
            {
                await rmHandler.DeleteAsync();
                await msgHandler.ModifyAsync("The message you are looking for does not exists.\n"
                    + "This message will be delete automatically in 3 2 1...");
                await Task.Delay(3000);
                await msgHandler.DeleteAsync();
            }
            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        }
        catch (DBClientTimeoutException)
        {
            try
            {
                // Database connection has timed out, abort the process.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
                await rmHandler.DeleteAsync();
            }
            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        }
    }
}

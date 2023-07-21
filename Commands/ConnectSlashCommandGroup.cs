using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Extensions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.CONNECT_COMMAND_NAME, CMD_CONSTANT.CONNECT_COMMAND_DESCRIPTION)]
public class ConnectSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.BUTTON_COMMAND_NAME, CMD_CONSTANT.BUTTON_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task ButtonForm(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(MessageIDAutocompleteProvider))]
        string messageID,
        [Option(CMD_CONSTANT.BUTTON_ID_PARAMETER, CMD_CONSTANT.BUTTON_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(ButtonIDAutocompleteProvider))]
        string buttonID,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();

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
                await ConnectCommandsModule.Connect(msgHandler, msgFound, buttonID, formID);
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
}

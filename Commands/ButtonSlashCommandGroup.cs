using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.BUTTON_COMMAND_NAME, CMD_CONSTANT.BUTTON_COMMAND_DESCRIPTION)]
public class ButtonSlashCommandGroup : ApplicationCommandModule
{
    #region Main

    [SlashCommand(CMD_CONSTANT.ADD_COMMAND_NAME, CMD_CONSTANT.BUTTON_ADD_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Add(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(MessageIDAutocompleteProvider))]
        string messageID)
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

                // Check if the target message is not the bot itself, this must be prevent due to Discord limitation.
                if (msgFound.Author.Id != ctx.Client.CurrentUser.Id)
                {
                    // Notify by message handler.
                    await msgHandler.ModifyAsync("Cannot target this user's message, abort the process.");
                    return;
                }

                // Set button process
                await ButtonCommandsModule.Add(ctx.Client.GetInteractivity(), ctx.User, msgHandler, msgFound);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Bad argument inserted to message ID, insert numbers only, abort the process.");
            }
            catch (NotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.GET_COMMAND_NAME, CMD_CONSTANT.BUTTON_GET_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Get(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(MessageIDAutocompleteProvider))]
        string messageID)
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
                DiscordMessage msgFound = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID), true);

                // Get and send information.
                await ButtonCommandsModule.Get(new DiscordEmbedBuilder.EmbedAuthor {
                    IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                    Name = ctx.Client.CurrentUser.Username,
                }, msgHandler, msgFound);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Bad argument inserted to message ID, insert numbers only, abort the process.");
            }
            catch (NotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.DELETE_COMMAND_NAME, CMD_CONSTANT.BUTTON_DELETE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Delete(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(MessageIDAutocompleteProvider))]
        string messageID,
        [Option(CMD_CONSTANT.BUTTON_ID_PARAMETER, CMD_CONSTANT.BUTTON_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(ButtonIDAutocompleteProvider))]
        string? buttonID = null)
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

                // Check if user did not provide the button ID, then make user choosing it.
                if (string.IsNullOrEmpty(buttonID))
                    buttonID = await ButtonCommandsModule.WaitForChoosingButton(ctx.User, msgHandler, msgFound);
                
                // Check if there's no respond from user.
                if (string.IsNullOrEmpty(buttonID))
                {
                    // Notify by message handler.
                    await msgHandler.ModifyAsync("No Respond from user, abort the process.");
                    return;
                }

                // Proceed deleting button from target message.
                await ButtonCommandsModule.Delete(msgHandler, msgFound, buttonID);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Bad argument inserted to message ID, insert numbers only, abort the process.");
            }
            catch (NotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
    
    #endregion
}
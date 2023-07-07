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
    public async Task SetButton(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, autocomplete: true)]
        [ChoiceProvider(typeof(MessageIDChoiceProvider))]
        string messageID)
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

            // Check if the target message is a user message, this must be prevent due to Discord limitation.
            if (!msgFound.Author.IsBot)
            {
                await msgHandler.ModifyAsync("Cannot target this user's message.\n"
                    + "This message will be delete automatically in 3 2 1...");
                await Task.Delay(3000);
                await msgHandler.DeleteAsync();
                await rmHandler.DeleteAsync();
                return;
            }

            // Set button process
            await ButtonCommandsModule.Add(ctx.Client.GetInteractivity(), ctx.User, msgHandler, msgFound);
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
        catch (NotFoundException)
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

    [SlashCommand(CMD_CONSTANT.GET_COMMAND_NAME, CMD_CONSTANT.BUTTON_GET_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task GetButton(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, autocomplete: true)]
        [ChoiceProvider(typeof(MessageIDChoiceProvider))]
        string messageID)
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

            // Get and send information.
            await ButtonCommandsModule.Get(new DiscordEmbedBuilder.EmbedAuthor {
                    IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                    Name = ctx.Client.CurrentUser.Username,
                }, msgHandler, msgFound);
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
        catch (NotFoundException)
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

    [SlashCommand(CMD_CONSTANT.DELETE_COMMAND_NAME, CMD_CONSTANT.BUTTON_DELETE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task RemoveButton(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION, autocomplete: true)]
        [ChoiceProvider(typeof(MessageIDChoiceProvider))]
        string messageID,
        [Option(CMD_CONSTANT.BUTTON_ID_PARAMETER, CMD_CONSTANT.BUTTON_ID_PARAMETER_DESCRIPTION, autocomplete: false)]
        string? buttonID = null)
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

            // Check if user did not provide the button ID, then make user choosing it.
            if (string.IsNullOrEmpty(buttonID))
                buttonID = await ButtonCommandsModule.WaitForChoosingButton(ctx.User, msgHandler, msgFound);
            
            // Check if there's no respond from user.
            if (string.IsNullOrEmpty(buttonID))
            {
                try
                {
                    await rmHandler.DeleteAsync();
                    await msgHandler.ModifyAsync("No Respond from user, abort the process.\n"
                        + "This message will be delete automatically in 3 2 1...");
                    await Task.Delay(3000);
                    await msgHandler.DeleteAsync();
                }
                catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
                return;
            }

            // Proceed deleting button from target message.
            await ButtonCommandsModule.Delete(msgHandler, msgFound, buttonID);
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
        catch (NotFoundException)
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
    
    #endregion
}
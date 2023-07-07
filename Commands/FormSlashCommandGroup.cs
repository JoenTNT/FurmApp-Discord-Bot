using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.FORM_COMMAND_NAME, CMD_CONSTANT.FORM_COMMAND_DESCRIPTION)]
public class FormSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.ADD_COMMAND_NAME, CMD_CONSTANT.ADD_FORM_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Add(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        string formID)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.Add(msgHandler, formID);
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }

    [SlashCommand(CMD_CONSTANT.GET_ALL_COMMAND_NAME, CMD_CONSTANT.GET_FORMS_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task GetAll(InteractionContext ctx)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.GetAll(msgHandler, new DiscordEmbedBuilder.EmbedAuthor {
                IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                Name = ctx.Client.CurrentUser.Username,
            });
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }
}
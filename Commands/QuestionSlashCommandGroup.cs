using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.QUESTION_COMMAND_NAME, CMD_CONSTANT.QUESTION_COMMAND_DESCRIPTION)]
public class QuestionSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.ADD_COMMAND_NAME, CMD_CONSTANT.QUESTION_ADD_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Add(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION)]
        string formID,
        [Option(CMD_CONSTANT.QUESTION_TEXT_PARAMETER, CMD_CONSTANT.QUESTION_TEXT_PARAMETER_DESCRIPTION)]
        string question,
        [Option(CMD_CONSTANT.QUESTION_REQUIRED_PARAMETER, CMD_CONSTANT.QUESTION_REQUIRED_PARAMETER_DESCRIPTION)]
        [Choice("True", "true")]
        [Choice("False", "false")]
        string required,
        [Option(CMD_CONSTANT.QUESTION_PLACEHOLDER_PARAMETER, CMD_CONSTANT.QUESTION_PLACEHOLDER_PARAMETER_DESCRIPTION)]
        string placeholder,
        [Option(CMD_CONSTANT.QUESTION_INPUT_STYLE_PARAMETER, CMD_CONSTANT.QUESTION_STYLE_PARAMETER_DESCRIPTION)]
        [Choice("Paragraph", "paragraph")]
        [Choice("Short", "short")]
        string style,
        [Option(CMD_CONSTANT.QUESTION_MIN_PARAMETER, CMD_CONSTANT.QUESTION_MIN_PARAMETER_DESCRIPTION)]
        string? min = null,
        [Option(CMD_CONSTANT.QUESTION_MAX_PARAMETER, CMD_CONSTANT.QUESTION_MAX_PARAMETER_DESCRIPTION)]
        string? max = null)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Fixing each input to correct types and then start the adding process.
            await QuestionCommandsModule.Add(msgHandler, formID, question,
                style == "short" ? TextInputStyle.Short : TextInputStyle.Paragraph, placeholder, bool.Parse(required),
                string.IsNullOrEmpty(min) ? 1 : int.Parse(min), string.IsNullOrEmpty(max) ? 512 : int.Parse(max));
            await rmHandler.DeleteAsync();
        }
        catch (FormatException) // Wrong input format.
        {
            try
            {
                await rmHandler.DeleteAsync();
                await msgHandler.ModifyAsync($"Bad argument detected, some insert parameters are incorrect, abort the process.\n"
                    + "This message will be delete automatically in 3 2 1...");
                await Task.Delay(3000);
                await msgHandler.DeleteAsync();
            }
            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
        }
    }
}
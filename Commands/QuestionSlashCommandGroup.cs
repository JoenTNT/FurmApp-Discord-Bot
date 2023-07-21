using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.QUESTION_COMMAND_NAME, CMD_CONSTANT.QUESTION_COMMAND_DESCRIPTION)]
public class QuestionSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.ADD_COMMAND_NAME, CMD_CONSTANT.QUESTION_ADD_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Add(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
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
                // Fixing each input to correct types and then start the adding process.
                await QuestionCommandsModule.Add(msgHandler, formID, question,
                    style == "short" ? TextInputStyle.Short : TextInputStyle.Paragraph, placeholder, bool.Parse(required),
                    string.IsNullOrEmpty(min) ? 1 : int.Parse(min), string.IsNullOrEmpty(max) ? 512 : int.Parse(max));
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Bad argument detected, some insert parameters are incorrect, abort the process.");
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.DELETE_COMMAND_NAME, CMD_CONSTANT.QUESTION_DELETE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Delete(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID,
        [Option(CMD_CONSTANT.QUESTION_NUMBER_PARAMETER, CMD_CONSTANT.QUESTION_NUMBER_PARAMETER_DESCRIPTION)]
        string questionNum)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();

            // Parsing input to question number.
            try
            {
                // Start deleting question by question number on target Form.
                await QuestionCommandsModule.Delete(msgHandler, formID, int.Parse(questionNum));
            }
            catch (FormatException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Bad input argument! Question number must start from 1, abort the process.");
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Selecting question number is out of range, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.EDIT_COMMAND_NAME, CMD_CONSTANT.QUESTION_EDIT_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Edit(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID,
        [Option(CMD_CONSTANT.QUESTION_NUMBER_PARAMETER, CMD_CONSTANT.QUESTION_NUMBER_PARAMETER_DESCRIPTION)]
        string questionNum,
        [Option(CMD_CONSTANT.QUESTION_TEXT_PARAMETER, CMD_CONSTANT.QUESTION_TEXT_PARAMETER_DESCRIPTION)]
        string? question = null,
        [Option(CMD_CONSTANT.QUESTION_REQUIRED_PARAMETER, CMD_CONSTANT.QUESTION_REQUIRED_PARAMETER_DESCRIPTION)]
        [Choice("True", "true")]
        [Choice("False", "false")]
        string? required = null,
        [Option(CMD_CONSTANT.QUESTION_PLACEHOLDER_PARAMETER, CMD_CONSTANT.QUESTION_PLACEHOLDER_PARAMETER_DESCRIPTION)]
        string? placeholder = null,
        [Option(CMD_CONSTANT.QUESTION_INPUT_STYLE_PARAMETER, CMD_CONSTANT.QUESTION_STYLE_PARAMETER_DESCRIPTION)]
        [Choice("Paragraph", "paragraph")]
        [Choice("Short", "short")]
        string? style = null,
        [Option(CMD_CONSTANT.QUESTION_MIN_PARAMETER, CMD_CONSTANT.QUESTION_MIN_PARAMETER_DESCRIPTION)]
        string? min = null,
        [Option(CMD_CONSTANT.QUESTION_MAX_PARAMETER, CMD_CONSTANT.QUESTION_MAX_PARAMETER_DESCRIPTION)]
        string? max = null)
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
                // Check input count, must be more than 1 inputs
                bool anyInput = question != null || required != null || placeholder != null || style != null || min != null || max != null;

                if (!anyInput)
                {
                    // Notify by message handler.
                    await msgHandler.ModifyAsync($"Bad argument input, must include question number & properties, abort the process.");
                    return;
                }

                // Check first input, must be question number.
                int questionNumber = 0;
                if (!int.TryParse(questionNum, out questionNumber))
                {
                    // Notify by message handler.
                    await msgHandler.ModifyAsync($"First argument (Question Number) must be number, abort the process.");
                    return;
                }

                // Start editing question.
                await QuestionCommandsModule.Edit(msgHandler, formID, questionNumber, question,
                    style == null ? null : (style.ToLower() == "short" ? TextInputStyle.Paragraph : TextInputStyle.Short),
                    placeholder, required == null ? null : bool.Parse(required),
                    min == null ? null : int.Parse(min), max == null ? null : int.Parse(max));
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Bad argument detected, some insert parameters are incorrect, abort the process.");
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Selecting question number is out of range, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }  
    }
}
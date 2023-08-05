#pragma warning disable CS8601
#pragma warning disable CS8625

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Utilities;

namespace FurmAppDBot.Commands;

public class QuestionCommandsModule : BaseCommandModule
{
    #region Variables

    /// <summary>
    /// Each form has questions limit due to database size limitation.
    /// </summary>
    public const int MAX_QUESTION_LIMIT = 10;

    #endregion

    #region Main

    [Command(CMD_CONSTANT.QUESTION_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Question(CommandContext ctx, string commandName, string formID, params string[] inputs)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Selecting the command by name.
                switch (commandName)
                {
                    case CMD_CONSTANT.ADD_COMMAND_NAME:
                        // Redefine all inputs when creating question.
                        object[] result; bool foundInput;
                        CreateDefinition(inputs, out result, out foundInput);

                        // Check any property not found, then abort the process.
                        if (!foundInput)
                        {
                            await msgHandler.ModifyAsync("Properties not found, may be false input, abort the process.");
                            return;
                        }

                        // Setting default values.
                        if (result[1] == null) result[1] = TextInputStyle.Paragraph;
                        if (result[3] == null) result[3] = true;
                        if (result[4] == null) result[4] = 1;
                        if (result[5] == null) result[5] = 512;
                        
                        // Required informations must be included.
                        if (result[0] == null) // Question parameter
                        {
                            // Notify interaction message.
                            await msgHandler.ModifyAsync("```[REQUIRED]\n"
                                + "Insert main question text for user to answer.\nSend a text message here to set.```");

                            // Wait for input.
                            (string, bool) results = await WaitForMessageInput(ctx.Client.GetInteractivity(), ctx.User, msgHandler);

                            // When timeout happens.
                            if (results.Item2)
                            {
                                // Notify by message handler.
                                await msgHandler.ModifyAsync($"Timeout! Abort the process.");
                                return;
                            }

                            // Set question text.
                            result[0] = results.Item1;
                        }
                        if (result[2] == null) // Placeholder parameter
                        {
                            // Notify interaction message.
                            await msgHandler.ModifyAsync("```[REQUIRED]\n"
                                + "Insert question placeholder text, tell a hint or an answer example for user.\n"
                                + "Send a text message here to set.```");
                            
                            // Wait for input.
                            (string, bool) res = await WaitForMessageInput(ctx.Client.GetInteractivity(), ctx.User, msgHandler);

                            // When timeout happens.
                            if (res.Item2)
                            {
                                // Notify by message handler.
                                await msgHandler.ModifyAsync($"Timeout! Abort the process.");
                                return;
                            }

                            // Set question text.
                            result[2] = res.Item1;
                        }

                        // Proceed inputs.
                        await Add(msgHandler, formID, (string)result[0], (TextInputStyle)result[1], (string)result[2],
                            (bool)result[3], (int)result[4], (int)result[5]);
                        break;
                    
                    case CMD_CONSTANT.DELETE_COMMAND_NAME:
                        // Declare initial question number.
                        int questionNum = 0;

                        // Parsing input to question number.
                        try { questionNum = int.Parse(inputs[0]); }
                        catch (FormatException)
                        {
                            // Notify by message handler.
                            await msgHandler.ModifyAsync($"Bad input argument! Question number must start from 1, abort the process.");
                            return;
                        }

                        // Start deleting question by question number on target Form.
                        await Delete(msgHandler, formID, questionNum);
                        break;
                    
                    case CMD_CONSTANT.EDIT_COMMAND_NAME:
                        // Check input count, must be more than 1 inputs
                        int inputlen = inputs.Length;
                        if (inputlen <= 1)
                        {
                            // Notify by message handler.
                            await msgHandler.ModifyAsync($"Bad argument input, must include question number & properties,"
                                + " abort the process.");
                            return;
                        }

                        // Check first input, must be question number.
                        int questionNumber = 0;
                        if (!int.TryParse(inputs[0], out questionNumber))
                        {
                            // Notify by message handler.
                            await msgHandler.ModifyAsync($"First argument (Question Number) must be number, abort the process.");
                            return;
                        }

                        // Redefine all inputs when creating question.
                        object[] r; string[] subInputs = new string[inputlen - 1]; bool fi;
                        Array.Copy(inputs, 1, subInputs, 0, inputlen - 1);
                        CreateDefinition(subInputs, out r, out fi);

                        // Check any property not found, then abort the process.
                        if (!fi)
                        {
                            await msgHandler.ModifyAsync("Properties not found, may be false input, abort the process.");
                            return;
                        }

                        // Start editing question.
                        await Edit(msgHandler, formID, questionNumber, (string?)r[0], (TextInputStyle?)r[1], (string?)r[2],
                            (bool?)r[3], (int?)r[4], (int?)r[5]);
                        break;

                    // TODO: Rearrange Questions Command.
                    case CMD_CONSTANT.SWAP_COMMAND_NAME:
                        break;
                }
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

    private void CreateDefinition(string[] elementValue, out object[] result, out bool foundAny)
    {
        // Initialize fix values.
        result = new object[6];
        foundAny = false;

        // Define question text.
        string? p0 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_TEXT_PARAMETER));
        if (p0 != null)
        {
            result[0] = SimpleUtility.GetValueAtFirstEqual(p0);
            if (!string.IsNullOrEmpty((string?)result[0])) foundAny = true;
        }

        // Define input text style.
        string? p1 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_INPUT_STYLE_PARAMETER));
        if (p1 != null)
        {
            result[1] = SimpleUtility.GetValueAtFirstEqual(p1);
            if (!string.IsNullOrEmpty((string?)result[1])) foundAny = true;

            // Convert to text input style.
            switch (((string)result[1]).ToLower())
            {
                case "paragraph" or "pg":
                    result[1] = TextInputStyle.Paragraph;
                    break;

                case "short" or "sh":
                    result[1] = TextInputStyle.Short;
                    break;
                
                default:
                    result[1] = null;
                    break;
            }
        }

        // Define question placeholder text.
        string? p2 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_PLACEHOLDER_PARAMETER));
        if (p2 != null)
        {
            result[2] = SimpleUtility.GetValueAtFirstEqual(p2);
            if (!string.IsNullOrEmpty((string?)result[2])) foundAny = true;
        }

        // Define question is required to be answer.
        string? p3 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_REQUIRED_PARAMETER));
        if (p3 != null)
        {
            result[3] = SimpleUtility.GetValueAtFirstEqual(p3);
            if (!string.IsNullOrEmpty((string?)result[3])) foundAny = true;

            // Convert to boolean.
            switch(((string)result[3]).ToLower())
            {
                case "true" or "t":
                    result[3] = true;
                    break;
                
                case "false" or "f":
                    result[3] = false;
                    break;

                default:
                    result[3] = null;
                    break;
            }
        }

        // Define answer minimal length.
        string? p4 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_MIN_PARAMETER));
        if (p4 != null)
        {
            result[4] = SimpleUtility.GetValueAtFirstEqual(p4);
            if (!string.IsNullOrEmpty((string?)result[4])) foundAny = true;

            // Convert to integer.
            try { result[4] = int.Parse((string)result[4]); }
            catch (FormatException) { result[4] = null; }
        }

        // Define answer maximal length.
        string? p5 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_MAX_PARAMETER));
        if (p5 != null)
        {
            result[5] = SimpleUtility.GetValueAtFirstEqual(p5);
            if (!string.IsNullOrEmpty((string?)result[5])) foundAny = true;

            // Convert to integer.
            try { result[5] = int.Parse((string)result[5]); }
            catch (FormatException) { result[5] = null; }
        }
    }

    private async Task<(string, bool)> WaitForMessageInput(InteractivityExtension interactivity, DiscordUser user, DiscordMessage msgHandler)
    {
        // Wait for user respond.
        var msg = await interactivity.WaitForMessageAsync((m) => {
            return m.Author.Id == user.Id && m.ChannelId == msgHandler.ChannelId;
        });
        
        // Check timeout trigger.
        if (msg.TimedOut) return (string.Empty, true);

        // Delete interaction respond by user.
        await msg.Result.DeleteAsync();
        
        // Return result.
        return (msg.Result.Content, false);
    }

    #endregion

    #region Statics

    /// <exception cref="FormNotFoundException">
    /// Specific case if form does not exists from database.
    /// </exception>
    public static async Task Add(DiscordMessage msgHandler, string formID, string question, TextInputStyle style,
        string placeholder, bool required, int minLength, int maxLength)
    {
        // Get form data from database.
        var form = await FormData.GetData(msgHandler.Channel.Guild.Id, formID);

        // Save data to database after adding new question.
        form.AddQuestion(question, style, placeholder, required, minLength, maxLength);
        await form.SaveData();

        // Notify saved content.
        await msgHandler.ModifyAsync($"Successfully added new question, now Form with ID `{formID}` "
            + $"has {form.QuestionCount} questions.");
    }

    /// <exception cref="FormNotFoundException">
    /// Specific case if form does not exists from database.
    /// </exception>
    public static async Task Delete(DiscordMessage msgHandler, string formID, int questionNum)
    {
        // Get form data from database.
        var form = await FormData.GetData(msgHandler.Channel.Guild.Id, formID);

        // Delete question from form by index (question number minus 1).
        form.RemoveQuestion(questionNum - 1);
        await form.SaveData();

        // Notify saved content.
        await msgHandler.ModifyAsync($"Successfully deleted question, now Form with ID `{formID}` "
            + $"has {form.QuestionCount} questions.");
    }

    public static async Task Edit(DiscordMessage msgHandler, string formID, int questionNum,
        string? question, TextInputStyle? style, string? placeholder, bool? required, int? minLength, int? maxLength)
    {
        // Change question property of form.
        FormData form = await FormData.GetData(msgHandler.Channel.Guild.Id, formID);
        form.SetQuestionProps(questionNum - 1, question, style, placeholder, required, minLength, maxLength);
        await form.SaveData();
        
        // Notify success message.
        var q = form[questionNum - 1];
        await  msgHandler.ModifyAsync($"[Form: {formID}]\nSuccessfully changed property the question number {questionNum}.\n"
            + $"```Question    : {q.Label}\n"
            + $"Style       : {(q.Style == TextInputStyle.Short ? "Short" : "Long")}\n"
            + $"Placeholder : {q.Placeholder}\n"
            + $"Required    : {q.Required}\n"
            + $"Min Letters : {q.MinimumLength}\n"
            + $"Max Letters : {q.MaximumLength}\n```");
    }

    #endregion
}
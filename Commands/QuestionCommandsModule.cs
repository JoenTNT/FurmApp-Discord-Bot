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
    #region Main

    [Command(CMD_CONSTANT.QUESTION_COMMAND_NAME)]
    public async Task Question(CommandContext ctx, string commandName, string formID, params string[] inputs)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            // Selecting the command by name.
            switch (commandName)
            {
                case CMD_CONSTANT.ADD_COMMAND_NAME:
                    // Redefine all inputs when creating question.
                    object[] result; bool foundInput;
                    CreateDefinition(inputs, out result, out foundInput);

                    // Check if inputs not found, then abort the process.
                    if (!foundInput)
                    {
                        try
                        {
                            await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists.\n"
                                + "This message will be delete automatically in 3 2 1...");
                            await Task.Delay(3000);
                            await msgHandler.DeleteAsync();
                        }
                        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
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
                        (string, bool) r = await WaitForMessageInput(ctx.Client.GetInteractivity(), ctx.User, msgHandler);

                        // When timeout happens.
                        if (r.Item2)
                        {
                            try
                            {
                                await msgHandler.ModifyAsync($"Timeout! Abort the process.\n"
                                    + "This message will be delete automatically in 3 2 1...");
                                await Task.Delay(3000);
                                await msgHandler.DeleteAsync();
                            }
                            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
                            return;
                        }

                        // Set question text.
                        result[0] = r.Item1;
                    }
                    if (result[2] == null) // Placeholder parameter
                    {
                        // Notify interaction message.
                        await msgHandler.ModifyAsync("```[REQUIRED]\n"
                            + "Insert question placeholder text, tell a hint or an answer example for user.\n"
                            + "Send a text message here to set.```");
                        
                        // Wait for input.
                        (string, bool) r = await WaitForMessageInput(ctx.Client.GetInteractivity(), ctx.User, msgHandler);

                        // When timeout happens.
                        if (r.Item2)
                        {
                            try
                            {
                                await msgHandler.ModifyAsync($"Timeout! Abort the process.\n"
                                    + "This message will be delete automatically in 3 2 1...");
                                await Task.Delay(3000);
                                await msgHandler.DeleteAsync();
                            }
                            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
                            return;
                        }

                        // Set question text.
                        result[2] = r.Item1;
                    }

                    // Proceed inputs.
                    await Add(msgHandler, formID, (string)result[0], (TextInputStyle)result[1], (string)result[2],
                        (bool)result[3], (int)result[4], (int)result[5]);
                    break;
                
                // case CMD_CONSTANT.GET_COMMAND_NAME when !string.IsNullOrEmpty(formID):
                //     await Get(msgHandler, formID);
                //     break;

                // case CMD_CONSTANT.DELETE_COMMAND_NAME:
                //     await Delete(ctx.User, msgHandler, formID);
                //     break;
            }
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            return;
        }
    }

    private void CreateDefinition(string[] elementValue, out object[] result, out bool foundAny)
    {
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
            catch (Exception) { result[4] = null; }
        }

        // Define answer maximal length.
        string? p5 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.QUESTION_MAX_PARAMETER));
        if (p5 != null)
        {
            result[5] = SimpleUtility.GetValueAtFirstEqual(p5);
            if (!string.IsNullOrEmpty((string?)result[5])) foundAny = true;

            // Convert to integer.
            try { result[5] = int.Parse((string)result[5]); }
            catch (Exception) { result[5] = null; }
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

    public static async Task Add(DiscordMessage msgHandler, string formID, string question, TextInputStyle style,
        string placeholder, bool required, int minLength, int maxLength)
    {
        // Check if form does exists.
        if (!(await FormInterfaceData.Exists(msgHandler.Channel.Guild.Id, formID)))
        {
            try
            {
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists.\n"
                    + "This message will be delete automatically in 3 2 1...");
                await Task.Delay(3000);
                await msgHandler.DeleteAsync();
            }
            catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
            return;
        }

        // Get form data from database.
        var form = await FormInterfaceData.GetData(msgHandler.Channel.Guild.Id, formID);

        // Save data to database after adding new question.
        form.AddQuestion(question, style, placeholder, required, minLength, maxLength);
        await form.SaveData();

        // Notify saved content.
        await msgHandler.ModifyAsync($"Successfully added new question, now Form with ID `{formID}` "
            + $"has {form.QuestionCount} questions.");
    }

    #endregion
}
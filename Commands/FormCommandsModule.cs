using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Commands;

public class FormCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.FORM_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Form(CommandContext ctx, string commandName, string? formID1 = null, string? formID2 = null)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Check form ID input.
                switch (commandName.ToLower())
                {
                    case CMD_CONSTANT.ADD_COMMAND_NAME when string.IsNullOrEmpty(formID1):
                    case CMD_CONSTANT.GET_COMMAND_NAME when string.IsNullOrEmpty(formID1):
                    case CMD_CONSTANT.DUPLICATE_COMMAND_NAME when string.IsNullOrEmpty(formID1):
                    case CMD_CONSTANT.RENAME_COMMAND_NAME when string.IsNullOrEmpty(formID1):
                        // Cancel process if form ID not input.
                        await msgHandler.ModifyAsync($"Please provide a Form ID, use `{CMD_CONSTANT.GET_ALL_COMMAND_NAME}` "
                            + "command to see all registered Forms.");
                        return;
                    case CMD_CONSTANT.DUPLICATE_COMMAND_NAME when string.IsNullOrEmpty(formID2):
                    case CMD_CONSTANT.RENAME_COMMAND_NAME when string.IsNullOrEmpty(formID2):
                        // Cancel process if form ID not input.
                        await msgHandler.ModifyAsync($"Please input second form ID for target command.");
                        return;
                }

                // Selecting the command by name.
                switch (commandName.ToLower())
                {
                    case CMD_CONSTANT.ADD_COMMAND_NAME when !string.IsNullOrEmpty(formID1):
                        await Add(msgHandler, formID1);
                        break;
                    
                    case CMD_CONSTANT.GET_COMMAND_NAME when !string.IsNullOrEmpty(formID1):
                        await Get(msgHandler, formID1);
                        break;

                    case CMD_CONSTANT.DELETE_COMMAND_NAME:
                        await Delete(ctx.User, msgHandler, formID1);
                        break;
                    
                    case CMD_CONSTANT.DUPLICATE_COMMAND_NAME when !string.IsNullOrEmpty(formID1) && !string.IsNullOrEmpty(formID2):
                        await Duplicate(msgHandler, formID1, formID2);
                        break;

                    case CMD_CONSTANT.RENAME_COMMAND_NAME when !string.IsNullOrEmpty(formID1) && !string.IsNullOrEmpty(formID2):
                        await Rename(msgHandler, formID1, formID2);
                        break;
                    
                    case CMD_CONSTANT.GET_ALL_COMMAND_NAME:
                        await GetAll(msgHandler, new DiscordEmbedBuilder.EmbedAuthor {
                            IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                            Name = ctx.Client.CurrentUser.Username,
                        });
                        break;
                }
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException) // When getting form is not found.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID1}` does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
        
    }

    #endregion

    #region Statics

    public static async Task Add(DiscordMessage msgHandler, string formID)
    {
        // Intialize initial data.
        var response = new DiscordMessageBuilder();

        try
        {
            // Checking if form already exists.
            await FormData.GetData(msgHandler.Channel.Guild.Id, SimpleUtility.ToDiscordChannelName(formID));
            response.WithContent($"Form `{formID}` has already been registered, abort the process.");
        }
        catch (FormNotFoundException)
        {
            // Creating form if not yet exists.
            await FormData.CreateData(msgHandler.Channel.Guild.Id, SimpleUtility.ToDiscordChannelName(formID));
            response.WithContent($"Successfully registered form with ID: `{formID}`");
        }

        // Notify the process result.
        await msgHandler.ModifyAsync(response);
    }

    public static async Task Get(DiscordMessage msgHandler, string formID)
    {
        // Intialize initial data.
        var msgBuilder = new DiscordMessageBuilder();
        var embed = new DiscordEmbedBuilder() {
            Author = new DiscordEmbedBuilder.EmbedAuthor {
                Name = msgHandler.Author.Username,
                IconUrl = msgHandler.Author.AvatarUrl,
            },
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            Title = $"Form Detail (ID: {formID})",
            Description = string.Empty,
        };

        // Receive information.
        var form = await FormData.GetData(msgHandler.Channel.Guild.Id, formID);
        embed.Description += $"```There are {form.QuestionCount} Question(s) in this form.\n"
            + $"You can create {QuestionCommandsModule.MAX_QUESTION_LIMIT - form.QuestionCount} more question(s).```";
        for (int i = 0; i < form.QuestionCount; i++)
        {
            embed.AddField($"{i + 1}. `{form[i].Label}`\n", $"┠-`placeholder`: {form[i].Placeholder}\n"
                + $"┠-[`style`: {form[i].Style}; `required`: {form[i].Required}; `min` = {form[i].MinimumLength}; `max` = {form[i].MaximumLength}]\n");
        }

        // Send result of form detail
        msgBuilder.Embed = embed;
        await msgHandler.ModifyAsync(msgBuilder);
    }

    public static async Task Delete(DiscordUser user, DiscordMessage msgHandler, string? formID = null)
    {
        // Check if user didn't input form ID.
        if (string.IsNullOrEmpty(formID))
        {
            // Wait for choosing a form to be deleted.
            (string, string) result = await WaitForChoosingFormID(user, msgHandler);

            // Assign form ID.
            formID = result.Item1;

            // Check if not choosing or empty interaction, then abort process.
            if (string.IsNullOrEmpty(formID))
            {
                try
                {
                    await msgHandler.ModifyAsync($"{result.Item2} abort the process.\n"
                        + "This message will be delete automatically in 3 2 1...");
                    await Task.Delay(3000);
                    await msgHandler.DeleteAsync();
                }
                catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
                return;
            }
        }

        try
        {
            // Checking if form already exists.
            await FormData.GetData(msgHandler.Channel.Guild.Id, SimpleUtility.ToDiscordChannelName(formID));

            // Comfirmation interaction to warn the user.
            msgHandler = await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent($"Are you sure you want to delete form `{formID}`?\n"
                    + $"**WARNING**: *Form will permanently deleted and you cannot restore it back.")
                .AddComponents(new DiscordComponent[] { 
                    new DiscordButtonComponent(ButtonStyle.Danger, "yes", "Yes"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "no", "No"),
                }));
            var result = await msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

            // Check timeout.
            if (result.TimedOut)
            {
                // Notify cancellation process.
                await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                    .WithContent($"Timeout! Abort the Process."));
                return;
            }

            // Check if process cancelled.
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            if (result.Result.Id == "no")
            {
                // Notify cancellation process.
                await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                    .WithContent($"Cancelled! Abort the Process."));
                return;
            }

            // Notify deletion process is running.
            await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent($"Deleting form with ID {formID}..."));

            // Deleting form in progress.
            bool succeed = await FormData.DeleteData(msgHandler.Channel.Guild.Id, formID);

            // Notify successful or unsuccessful deletion process.
            if (succeed) await msgHandler.ModifyAsync("Form has been successfully deleted!");
            else await msgHandler.ModifyAsync("Unsuccessfully delete a form, something may be wrong, abort the process.");
        }
        catch (FormNotFoundException)
        {
            // Notify form not found.
            await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent("Form doesn't exists, abort deletion process."));
            return;
        }
    }

    public static async Task<(string, string)> WaitForChoosingFormID(DiscordUser user, DiscordMessage msgHandler)
    {
        // Receive database instance and declare pagination variables.
        var db = MainDatabase.Instance;
        int page = 0, limitData = 10; // TODO: Paginating interaction
        List<BsonDocument> docs = new();

        // Handle database timeout when using it.
        await db.HandleDBProcess(async () => {
            // Init collection from database.
            var collection = await db.InitCollection(msgHandler.Channel.Guild.Id, DB_CONSTANT.FORM_DATABASE_NAME);

            // Get all collection information.
            var filter = Builders<BsonDocument>.Filter.Empty;
            docs.Clear();
            docs.AddRange(await collection.Find(filter).Skip(page * limitData).Limit(limitData).ToListAsync());
        });

        // Check if there is no form registered, then abort process.
        if (docs.Count == 0) return (string.Empty, "Form list is empty.");

        // Create interaction message.
        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.Content = $"```[Page {page + 1}]\nChoose the Form you want to delete\n\n";
        var btnComps = new List<DiscordButtonComponent>() {
            new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel") };
        for (int i = 0; i < docs.Count; i++)
        {
            // Add descriptions.
            msgBuilder.Content += $"{i + 1}. {docs[i][DB_CONSTANT.FORM_ID_KEY].AsString} "
                + $"(Questions Count: {docs[i][DB_CONSTANT.FORM_QUESTIONS_KEY].AsBsonArray.Count})\n";

            // Add button interaction.
            btnComps.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"b{i + 1}", $"{i + 1}"));
        }
        msgBuilder.Content += "```";
        msgBuilder.AddComponents(btnComps.ToArray());

        // Start waiting for user to choose one of the form ID using button.
        msgHandler = await msgHandler.ModifyAsync(msgBuilder);
        var pickedBtn = await msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Check if interaction timed out.
        if (pickedBtn.TimedOut) return (string.Empty, "Timeout!");

        // Respond to button interaction.
        await pickedBtn.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

        // Check if interaction has been canceled.
        if (pickedBtn.Result.Id == "cancel") return (string.Empty, "Cancel deletion process.");

        // Select and return the form ID value.
        return (docs[int.Parse(pickedBtn.Result.Id.Substring(1)) - 1][DB_CONSTANT.FORM_ID_KEY].AsString, string.Empty);
    }

    public static async Task Duplicate(DiscordMessage msgHandler, string formSource, string formDupe)
    {
        // TODO: Duplicate form to new one.
        await Task.CompletedTask;
    }

    public static async Task Rename(DiscordMessage msgHandler, string formSource, string formRename)
    {
        // TODO: Rename form with new ID.
        await Task.CompletedTask;
    }

    public static async Task GetAll(DiscordMessage msgHandler, DiscordEmbedBuilder.EmbedAuthor author)
    {
        // Receive database instance and declare pagination variables.
        var db = MainDatabase.Instance;
        int page = 0, limitData = 10; // TODO: Paginating interaction

        // Create message and embed placeholder.
        var msgBuilder = new DiscordMessageBuilder();
        var embed = new DiscordEmbedBuilder() {
            Author = author,
            Description = string.Empty,
            Title = $"List of Forms (Page {page + 1})",
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
        };

        // Handle database timeout when using it.
        await db.HandleDBProcess(async () => {
            // Init collection from database.
            var collection = await db.InitCollection(msgHandler.Channel.Guild.Id, DB_CONSTANT.FORM_DATABASE_NAME);

            // Get all collection information.
            var filter = Builders<BsonDocument>.Filter.Empty;
            var documents = await collection.Find(filter).Skip(page * limitData).Limit(limitData).ToListAsync();

            // Check if there are no form registered yet, then inform special case.
            if (documents.Count == 0)
            {
                embed.Title = "No Forms Yet";
                embed.Description = $"Currently there are no form registered, "
                    + $"consider to create one using command `{CMD_CONSTANT.FORM_COMMAND_NAME} {CMD_CONSTANT.ADD_COMMAND_NAME}`";
                return;
            }

            // Summarize informations.
            string formID;
            int formCount = 0;
            foreach (var doc in documents)
            {
                formID = doc[DB_CONSTANT.FORM_ID_KEY].AsString;
                formCount++;
                embed.Description += $"> {formCount}. `{formID}`\n";
            }
        });

        // Notify result.
        msgBuilder.AddEmbed(embed);
        await msgHandler.ModifyAsync(msgBuilder);
    }

    #endregion
}
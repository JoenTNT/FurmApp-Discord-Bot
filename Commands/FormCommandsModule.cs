using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Commands;

public class FormCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.FORM_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Form(CommandContext ctx, string commandName, string? formID = null)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            // Check form ID input.
            switch (commandName.ToLower())
            {
                case CMD_CONSTANT.ADD_COMMAND_NAME when string.IsNullOrEmpty(formID):
                case CMD_CONSTANT.GET_COMMAND_NAME when string.IsNullOrEmpty(formID):
                case CMD_CONSTANT.DELETE_COMMAND_NAME when string.IsNullOrEmpty(formID):
                    // Cancel process if form ID not input.
                    await msgHandler.ModifyAsync($"Please provide a Form ID, use `{CMD_CONSTANT.GET_ALL_COMMAND_NAME}` "
                        + "command to see all registered Forms.");
                    return;
            }

            // Selecting the command by name.
            switch (commandName)
            {
                case CMD_CONSTANT.ADD_COMMAND_NAME when !string.IsNullOrEmpty(formID):
                    await Add(msgHandler, formID);
                    break;
                
                case CMD_CONSTANT.GET_COMMAND_NAME when !string.IsNullOrEmpty(formID):
                    await Get(msgHandler, formID);
                    break;

                case CMD_CONSTANT.DELETE_COMMAND_NAME when !string.IsNullOrEmpty(formID):
                    await Delete(msgHandler, formID);
                    break;
                
                case CMD_CONSTANT.GET_ALL_COMMAND_NAME:
                    await GetAll(msgHandler, new DiscordEmbedBuilder.EmbedAuthor {
                        IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                        Name = ctx.Client.CurrentUser.Username,
                    });
                    break;
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

    #endregion

    #region Statics

    public static async Task Add(DiscordMessage msgHandler, string formID)
    {
        // Intialize initial data.
        var db = MainDatabase.Instance;
        var response = new DiscordMessageBuilder();

        // Check if form with ID already exists.
        if (await FormInterfaceData.Exists(msgHandler.Channel.Guild.Id, formID))
        {
            // Notify with abort process.
            response.WithContent($"Form `{formID}` has already been registered, abort the process.");
        }
        else
        {
            // Create and save new data.
            var d = await FormInterfaceData.GetData(msgHandler.Channel.Guild.Id, formID);
            await d.SaveData();

            // notify with success message.
            response.WithContent($"Successfully registered form with ID: `{formID}`");
        }

        // Notify the process result.
        await msgHandler.ModifyAsync(response);
    }

    public static async Task Get(DiscordMessage msgHandler, string formID)
    {
        await Task.CompletedTask;
    }

    public static async Task Delete(DiscordMessage msgHandler, string formID)
    {
        await Task.CompletedTask;
    }

    public static async Task GetAll(DiscordMessage msgHandler, DiscordEmbedBuilder.EmbedAuthor author)
    {
        var db = MainDatabase.Instance;
        int page = 0, limitData = 10; // TODO: Paginating interaction

        var msgBuilder = new DiscordMessageBuilder();
        var embed = new DiscordEmbedBuilder() {
            Author = author,
            Description = string.Empty,
            Title = $"List of Forms (Page {page + 1})",
        };

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

        msgBuilder.AddEmbed(embed);
        await msgHandler.ModifyAsync(msgBuilder);
    }

    #endregion
}
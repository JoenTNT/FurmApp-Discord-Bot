using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Commands;

public static class FormCommand
{
    public static async Task AddForm(InteractionContext ctx, string formID)
    {
        // Handle a long process when interacting
        await ctx.DeferAsync(true);

        // Intialize initial data.
        var db = MainDatabase.Instance;
        var response = new DiscordWebhookBuilder();

        try
        {
            //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Start Checking for existing form...");
            if (await FormInterfaceData.Exists(ctx.Guild.Id, formID))
            {
                //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Already Exists!");
                response.WithContent($"Form `{formID}` has already been registered, abort the process.");
            }
            else
            {
                //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Not yet exists!");
                var d = await FormInterfaceData.GetData(ctx.Guild.Id, formID);
                await d.SaveData();

                response.WithContent($"Successfully registered form with ID: `{formID}`");
            }
        }
        catch (DBClientTimeoutException)
        {
            response.WithContent("```Request Time Out, please try again later!```");
        }
        
        await ctx.Interaction.EditOriginalResponseAsync(response);
    }

    public static async Task AddForm(CommandContext ctx, string formID)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("please wait for a moment...");

        // Intialize initial data.
        var db = MainDatabase.Instance;
        var response = new DiscordMessageBuilder();

        try
        {
            //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Start Checking for existing form...");
            if (await FormInterfaceData.Exists(ctx.Guild.Id, formID))
            {
                //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Already Exists!");
                response.WithContent($"Form `{formID}` has already been registered, abort the process.");
            }
            else
            {
                //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Not yet exists!");
                var d = await FormInterfaceData.GetData(ctx.Guild.Id, formID);
                await d.SaveData();

                response.WithContent($"Successfully registered form with ID: `{formID}`");
            }
        }
        catch (DBClientTimeoutException)
        {
            response.WithContent("```Request Time Out, please try again later!```");
        }
        
        await msgHandler.ModifyAsync(response);
    }

    public static async Task DeleteForm(InteractionContext ctx)
    {
        await Task.CompletedTask;
    }

    public static async Task GetAllForm(InteractionContext ctx)
    {
        var db = MainDatabase.Instance;
        int page = 0, limitData = 10;

        await ctx.DeferAsync(true);

        var response = new DiscordWebhookBuilder();
        var embed = new DiscordEmbedBuilder() {
            Author = new DiscordEmbedBuilder.EmbedAuthor {
                IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                Name = ctx.Client.CurrentUser.Username,
            },
            Description = string.Empty,
            Title = $"List of Forms (Page {page + 1})",
        };

        try
        {
            await db.HandleDBProcess(async () => {
                // Init collection from database.
                var collection = await db.InitCollection(ctx.Guild.Id, DB_CONSTANT.FORM_DATABASE_NAME);

                // Get all collection information.
                var filter = Builders<BsonDocument>.Filter.Empty;
                var documents = await collection.Find(filter).Skip(page * limitData).Limit(limitData).ToListAsync();

                if (documents.Count == 0)
                {
                    embed.Title = "No Forms Yet";
                    embed.Description = $"Currently there are no form registered, "
                        + $"consider to create one using command `{CMD_CONSTANT.ADD_FORM_COMMAND_NAME}`";
                    return;
                }

                string formID;
                int formCount = 0;
                foreach (var doc in documents)
                {
                    formID = doc[DB_CONSTANT.FORM_ID_KEY].AsString;
                    formCount++;
                    embed.Description += $"> {formCount}. `{formID}`\n";
                }
            });
        }
        catch (DBClientTimeoutException)
        {
            response.WithContent("```Request Time Out, please try again later!```");
            await ctx.Interaction.EditOriginalResponseAsync(response);
        }

        response.AddEmbed(embed);
        await ctx.Interaction.EditOriginalResponseAsync(response);
    }
}
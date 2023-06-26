using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Commands;

public static class FormCommand
{
    public static async Task AddForm(InteractionContext ctx, string formID)
    {
        var db = MainDatabase.Instance;

        await ctx.DeferAsync(true);

        var response = new DiscordWebhookBuilder();

        try
        {
            await db.HandleDBProcess(async () => {
                // Initialize database and collection
                var client = await db.GetClient();
                var database = client.GetDatabase(DB_CONSTANT.FORM_DATABASE_NAME);
                var collection = database.GetCollection<BsonDocument>($"{ctx.Guild.Id}");

                // Check if for registered, then abort the process
                var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.FORM_ID_KEY, formID);
                var foundDoc = await collection.Find(filter).ToListAsync();

                if (foundDoc.Count > 0)
                {
                    // Abort the process
                    response.WithContent($"Form `{formID}` has already been registered, abort the process.");
                    return;
                }

                // Register form into the database
                await collection.InsertOneAsync(new BsonDocument {
                    { DB_CONSTANT.FORM_ID_KEY, formID },
                    { DB_CONSTANT.FORM_QUESTIONS_KEY, new BsonDocument() },
                });

                response.WithContent($"Successfully registered form with ID: `{formID}`");
            });
        }
        catch (DBClientTimeoutException)
        {

        }
        
        
        await ctx.Interaction.EditOriginalResponseAsync(response);
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

        await db.HandleDBProcess(async () => {
            var client = await db.GetClient();
            var database = client.GetDatabase(DB_CONSTANT.FORM_DATABASE_NAME);
            var collection = database.GetCollection<BsonDocument>($"{ctx.Guild.Id}");

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

        response.AddEmbed(embed);
        await ctx.Interaction.EditOriginalResponseAsync(response);
    }
}
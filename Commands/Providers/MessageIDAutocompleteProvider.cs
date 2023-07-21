using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Commands.Providers;

public class MessageIDAutocompleteProvider : IAutocompleteProvider
{
    #region ChoiceProvider

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        return GetAllMessageID(ctx.Channel);
    }

    #endregion

    #region Main

    private async Task<IEnumerable<DiscordAutoCompleteChoice>> GetAllMessageID(DiscordChannel channel)
    {
        // Receive database instance.
        MainDatabase db = MainDatabase.Instance;

        // Declare list of hint.
        List<DiscordAutoCompleteChoice> choices = new();

        try
        {
            // Receive message informations from database.
            await db.HandleDBProcess(async () => {
                // Get collection from database.
                var collection = await db.InitCollection(channel.Guild.Id, DB_CONSTANT.INTERFACE_DATABASE_NAME);

                // Filtering by limiting results.
                var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, $"{channel.Id}");
                var projection = Builders<BsonDocument>.Projection.Include(DB_CONSTANT.MESSAGE_ID_KEY);
                var docs = await collection.Find(filter).Project(projection).ToListAsync();

                // Check if not exists, return empty.
                if (docs.Count <= 0) return;

                // Collecting it to list.
                foreach (var nm in docs[0][DB_CONSTANT.MESSAGE_ID_KEY].AsBsonDocument.Names)
                    choices.Add(new DiscordAutoCompleteChoice(nm, nm));
            });
        }
        catch (DBClientTimeoutException) { /* Ignore timeout. */ }
        
        // Return message ID choices.
        return choices;
    }

    #endregion
}

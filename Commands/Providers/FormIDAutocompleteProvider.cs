using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using MongoDB.Bson;
using MongoDB.Driver;

namespace  FurmAppDBot.Commands.Providers;

public class FormIDAutocompleteProvider : IAutocompleteProvider
{
    #region ChoiceProvider

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        return GetAllForms(ctx.Guild);
    }

    #endregion

    #region Main

    private async Task<IEnumerable<DiscordAutoCompleteChoice>> GetAllForms(DiscordGuild guild)
    {
        // Receive database instance.
        MainDatabase db = MainDatabase.Instance;

        // Declare options provider.
        var l = new List<DiscordAutoCompleteChoice>();

        // Handle database process which timeout may occur.
        await db.HandleDBProcess(async () => {
            // Initialize collection.
            var collection = await db.InitCollection(guild.Id, DB_CONSTANT.FORM_DATABASE_NAME);

            // Receive by filtering and projecting information.
            var projection = Builders<BsonDocument>.Projection.Include(DB_CONSTANT.FORM_ID_KEY);
            var docs = await collection.Find(Builders<BsonDocument>.Filter.Empty).Project(projection).ToListAsync();

            // Insert all Form ID information.
            foreach (var d in docs)
                l.Add(new DiscordAutoCompleteChoice(d[DB_CONSTANT.FORM_ID_KEY].AsString,
                    d[DB_CONSTANT.FORM_ID_KEY].AsString));
        });

        // Return provider result.
        return l;
    }

    #endregion
}
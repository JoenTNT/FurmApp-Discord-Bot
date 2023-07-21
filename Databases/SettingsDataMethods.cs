using DSharpPlus.Entities;
using FurmAppDBot.Clients;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Databases;

public static class SettingsDataMethods
{
    #region Variables

    private const string OBJID_KEY = "_id";

    #endregion

    #region Main

    /// <summary>
    /// Get channel category ID information.
    /// </summary>
    /// <param name="guild">Target guild</param>
    /// <returns>Channel category ID</returns>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task<ulong?> GetCategoryAsContainer(DiscordGuild guild)
    {
        // Receive database instance.
        var db = MainDatabase.Instance;

        // Handling timeout database process.
        return await db.HandleDBProcess<ulong?>(async () => {
            // Get collection.
            var collection = await db.InitCollection(guild.Id, DB_CONSTANT.SETTING_DATABASE_NAME);

            // Get the only document in that collection.
            var doc = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            // Check if document not yet created.
            BsonDocument settingDoc;
            if (doc.Count <= 0)
            {
                // Create default settings value, focuses at category container.
                settingDoc = new BsonDocument { { DB_CONSTANT.CHANNEL_AND_CATEGORY_AS_CONTAINER_KEY, BsonNull.Value },};
                await collection.InsertOneAsync(settingDoc);
            }
            else
            {
                // Get the only document.
                settingDoc = doc[0];
            }

            // Convert to readable data.
            BsonValue category = settingDoc[DB_CONSTANT.CHANNEL_AND_CATEGORY_AS_CONTAINER_KEY];
            return category == BsonNull.Value ? null : ulong.Parse(category.AsString);
        });
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task SetCategoryAsContainer(DiscordGuild guild, ulong ccID)
    {
        // Receive database instance.
        var db = MainDatabase.Instance;

        // Handling timeout database process.
        await db.HandleDBProcess(async () => {
            // Get collection.
            var collection = await db.InitCollection(guild.Id, DB_CONSTANT.SETTING_DATABASE_NAME);
            // Get the only document in that collection.
            var doc = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            // Check if document not yet created.
            if (doc.Count <= 0)
            {
                // Create default settings value, focuses at category container.
                await collection.InsertOneAsync(new BsonDocument {
                    { DB_CONSTANT.CHANNEL_AND_CATEGORY_AS_CONTAINER_KEY, $"{ccID}" },
                });
                return;
            }
            
            // Update the target document.
            var filter = Builders<BsonDocument>.Filter.Eq(OBJID_KEY, doc[0][OBJID_KEY].AsObjectId);
            await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set(
                DB_CONSTANT.CHANNEL_AND_CATEGORY_AS_CONTAINER_KEY, $"{ccID}"));
        });
    }

    #endregion
}
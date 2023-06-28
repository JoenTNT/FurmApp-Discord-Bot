using FurmAppDBot.Clients;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Databases;

/// <summary>
/// Button interface data that tracks the message and the button.
/// </summary>
[Serializable]
public sealed class ButtonInterfaceData : DataElementBase, ILoadDatabase, ISaveDatabase
{
    #region Variables

    private static HashSet<ButtonInterfaceData> s_cacheData = new();

    private ulong _guildID = 0;
    private ulong _channelID = 0;
    private Dictionary<string, Dictionary<string, string>> _messageButtonForm = new();

    #endregion

    #region Properties

    public static HashSet<ButtonInterfaceData> CacheData => s_cacheData;

    public IReadOnlyDictionary<string, string>? this[string messageID]
    {
        get
        {
            if (!_messageButtonForm.ContainsKey(messageID))
                return null;

            return _messageButtonForm[messageID];
        }
    }

    public ulong GuildID => _guildID;

    public ulong ChannelID => _channelID;

    #endregion

    #region Constructor

    private ButtonInterfaceData(MainDatabase databaseRef) : base(databaseRef) { }

    private ButtonInterfaceData(MainDatabase databaseRef, ulong initialGuildID, ulong initialChannelID)
        : base(databaseRef)
    {
        _guildID = initialGuildID;
        _channelID = initialChannelID;
    }

    #endregion

    #region ILoadDatabase

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task<bool> LoadData()
    {
        await DatabaseRef.HandleDBProcess(async () => {
            // Initialize database and collection.
            var collection = await DatabaseRef.InitCollection(_guildID, DB_CONSTANT.INTERFACE_DATABASE_NAME);

            // Get the data result.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, $"{_channelID}");
            var result = await collection.Find(filter).ToListAsync();

            // Check if data not found.
            if (result.Count == 0) return;

            // Receive all the information
            foreach (var d in result[0][DB_CONSTANT.MESSAGE_ID_KEY].AsBsonDocument)
            {
                if (!_messageButtonForm.ContainsKey(d.Name))
                    _messageButtonForm[d.Name] = new();
                
                foreach (var b in d.Value.AsBsonDocument)
                    _messageButtonForm[d.Name][b.Name] = b.Value.AsString;
            }
        });

        return true;
    }

    #endregion

    #region ISaveDatabase

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task<bool> SaveData()
    {
        var buttonsDoc = new BsonDocument(_messageButtonForm);

        await DatabaseRef.HandleDBProcess(async () => {
            // Check if the collection exists
            var collection = await DatabaseRef.InitCollection(_guildID, DB_CONSTANT.INTERFACE_DATABASE_NAME);

            // Get the data result.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, $"{_channelID}");
            var channelDocumentFound = await collection.Find(filter).ToListAsync();

            if (channelDocumentFound.Count == 0)
            {
                //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Channel not found, inserting data...");
                await collection.InsertOneAsync(new BsonDocument {
                    { DB_CONSTANT.CHANNEL_ID_KEY, $"{_channelID}" },
                    { DB_CONSTANT.MESSAGE_ID_KEY, new BsonDocument() },
                });
                //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] New channel document has been inserted!");
            }

            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Updating data...");
            await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set(DB_CONSTANT.MESSAGE_ID_KEY, buttonsDoc));
        });

        return true;
    }

    #endregion

    #region Main

    public bool ContainsMessage(string messageID) => _messageButtonForm.ContainsKey(messageID);

    public bool ContainsButtonInMessage(string messageID, string buttonID)
    {
        if (!ContainsMessage(messageID)) return false;

        return _messageButtonForm[messageID].ContainsKey(buttonID);
    }

    public void AddButton(string messageID, string buttonID)
    {
        if (!_messageButtonForm.ContainsKey(messageID))
            _messageButtonForm[messageID] = new();

        _messageButtonForm[messageID][buttonID] = string.Empty;
    }

    public bool DeleteButton(string messageID, string buttonID)
    {
        if (!_messageButtonForm.ContainsKey(messageID)) return false;

        _messageButtonForm[messageID].Remove(buttonID);

        // Delete message ID if there's no button registered in that message.
        if (_messageButtonForm[messageID].Count == 0)
            _messageButtonForm.Remove(messageID);

        return true;
    }

    /// <summary>
    /// Get interface data.
    /// Automatically creates a new data when not exists.
    /// </summary>
    /// <param name="guildID">Target server ID</param>
    /// <param name="channelID">Target channel ID of the server</param>
    /// <returns>Loaded interface data</returns>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exceptiion>
    public static async Task<ButtonInterfaceData> GetData(ulong guildID, ulong channelID)
    {
        ButtonInterfaceData data;
        Func<ButtonInterfaceData, bool> q = (i) => i.GuildID == guildID && i.ChannelID == channelID;

        // Check if there's any cache information.
        if (!s_cacheData.Any(q))
        {
            data = new ButtonInterfaceData(MainDatabase.Instance, guildID, channelID);
            s_cacheData.Add(data);
            await data.LoadData();
            return data;
        }

        data = s_cacheData.First(q);
        return data;
    }

    public static void ClearCache() => s_cacheData.Clear();

    #endregion
}
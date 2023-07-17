using FurmAppDBot.Clients;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FurmAppDBot.Databases;

/// <summary>
/// Button interface data that tracks the message and the button.
/// </summary>
[Serializable]
public sealed class InterfaceData : DataElementBase, ILoadDatabase, ISaveDatabase
{
    #region Variables

    private ulong _guildID = 0;
    private ulong _channelID = 0;
    private Dictionary<string, Dictionary<string, Dictionary<string, string>>> _formsInMessage = new();

    #endregion

    #region Properties

    public ulong GuildID => _guildID;

    public ulong ChannelID => _channelID;

    #endregion

    #region Constructor

    private InterfaceData(MainDatabase databaseRef, ulong initialGuildID, ulong initialChannelID)
        : base(databaseRef)
    {
        _guildID = initialGuildID;
        _channelID = initialChannelID;
    }

    #endregion

    #region ILoadDatabase

    /// <summary>
    /// WARNING: This will replace the questions data, do not run unless you have backed up the data.
    /// </summary>
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

            // Clear old information.
            _formsInMessage.Clear();

            // Receive all informations, specifically message IDs.
            foreach (var d in result[0][DB_CONSTANT.MESSAGE_ID_KEY].AsBsonDocument)
            {
                // Create message collection.
                _formsInMessage[d.Name] = new();

                // Loop through interface types.
                foreach (var it in d.Value.AsBsonDocument)
                {
                    // Create interface type collection.
                    _formsInMessage[d.Name][it.Name] = new();

                    // Loop through component names.
                    foreach (var b in it.Value.AsBsonDocument)
                        _formsInMessage[d.Name][DB_CONSTANT.BUTTONS_KEY][b.Name] = b.Value.AsString;
                }
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
        await DatabaseRef.HandleDBProcess(async () => {
            // Check if the collection exists
            var collection = await DatabaseRef.InitCollection(_guildID, DB_CONSTANT.INTERFACE_DATABASE_NAME);

            // Get the data result.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, $"{_channelID}");
            var channelDocumentFound = await collection.Find(filter).ToListAsync();

            // Check if the data is empty.
            if (_formsInMessage.Count == 0 && channelDocumentFound.Count > 0)
            {
                // Delete document from collection.
                await collection.DeleteOneAsync(filter);
                return;
            }

            // Check if there's no document in collection.
            if (channelDocumentFound.Count == 0)
            {
                // Add new one button interface data
                await collection.InsertOneAsync(new BsonDocument {
                    { DB_CONSTANT.CHANNEL_ID_KEY, $"{_channelID}" },
                    { DB_CONSTANT.MESSAGE_ID_KEY, new BsonDocument() },
                });
            }

            // Update interface data to database.
            await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set(DB_CONSTANT.MESSAGE_ID_KEY, _formsInMessage));
        });

        return true;
    }

    #endregion

    #region Main

    public async override Task ConnectElement(DataElementBase with, params string[] keys)
    {
        if (with is FormData)
        {
            // Convert to form interface data
            var formData = (FormData)with;

            // First index is the message ID, second index is the button ID.
            this.SetButtonFormID(keys[0], keys[1], formData.FormID);
            await this.SaveData();
            return;
        }

        throw new ConnectElementFailedException();
    }

    /// <summary>
    /// Get registered button interfaces and forms.
    /// </summary>
    /// <param name="messageID">Target message ID</param>
    /// <returns>Returns null if message or button unregistered.</returns>
    public IReadOnlyDictionary<string, string>? GetButtons(string messageID)
    {
        if (!_formsInMessage.ContainsKey(messageID)) return null;
        if (!_formsInMessage[messageID].ContainsKey(DB_CONSTANT.BUTTONS_KEY)) return null;

        return _formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY];
    }

    internal bool SetButtonFormID(string messageID, string buttonID, string formID)
    {
        // Check if keys not yet registered, then register it.
        if (!_formsInMessage.ContainsKey(messageID)) return false;
        if (!_formsInMessage[messageID].ContainsKey(DB_CONSTANT.BUTTONS_KEY)) return false;
        
        // Set form id on target registered button.
        _formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY][buttonID] = formID;

        // Return successful setting form process.
        return true;
    }

    public void AddButton(string messageID, string buttonID)
    {
        // Check if keys not yet registered, then register it.
        if (!_formsInMessage.ContainsKey(messageID)) _formsInMessage[messageID] = new();
        if (!_formsInMessage[messageID].ContainsKey(DB_CONSTANT.BUTTONS_KEY))
            _formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY] = new();

        // Register button interface equals empty form.
        _formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY][buttonID] = string.Empty;
    }

    public bool DeleteButton(string messageID, string buttonID)
    {
        // Check if message has data.
        if (!_formsInMessage.ContainsKey(messageID)) return false;
        if (!_formsInMessage[messageID].ContainsKey(DB_CONSTANT.BUTTONS_KEY)) return false;

        // Remove pair of button and form.
        _formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY].Remove(buttonID);

        // Delete message ID if there's no button registered in that message.
        if (_formsInMessage[messageID][DB_CONSTANT.BUTTONS_KEY].Count == 0)
            _formsInMessage[messageID].Remove(DB_CONSTANT.BUTTONS_KEY);
        if (_formsInMessage[messageID].Count == 0)
            _formsInMessage.Remove(messageID);

        // Return successful deletion process.
        return true;
    }

    private static async Task<bool> Exists(ulong guildID, ulong channelID)
    {
        // Search via cache.
        var db = MainDatabase.Instance;
        var result = false;

        await db.HandleDBProcess(async () => {
            // Init collection.
            var collection = await db.InitCollection(guildID, DB_CONSTANT.INTERFACE_DATABASE_NAME);

            // Find data from database collection using filter.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, $"{channelID}");
            result = await collection.Find(filter).AnyAsync();
        });

        return result;
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
    /// </exception>
    /// <exception cref="InterfaceUnregisteredException">
    /// When interface not registered in database.
    /// </exception>
    public static async Task<InterfaceData> GetData(ulong guildID, ulong channelID)
    {
        // Check if the interface does not registered in database.
        if (!(await Exists(guildID, channelID)))
            throw new InterfaceUnregisteredException(guildID, channelID);

        // Immediately load data from database.
        InterfaceData data = new InterfaceData(MainDatabase.Instance, guildID, channelID);
        await data.LoadData();
        return data;
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task<InterfaceData> CreateData(ulong guildID, ulong channelID)
    {
        // Declare data.
        InterfaceData data;

        // Check if form exists.
        if (await Exists(guildID, channelID))
        {
            data = new InterfaceData(MainDatabase.Instance, guildID, channelID);
            await data.LoadData();
            return data;
        }
        
        // Immediately create data and save it to database.
        data = new InterfaceData(MainDatabase.Instance, guildID, channelID);
        await data.SaveData();
        return data;
    }

    #endregion
}
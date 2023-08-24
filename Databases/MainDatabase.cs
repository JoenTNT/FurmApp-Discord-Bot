#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.Exceptions;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Databases;

public class MainDatabase
{
    #region Variables

    private const int LIMIT_TRIES = 10;

    private static MainDatabase? s_instance;

    //private const string CONNECTION_URL = "mongodb+srv://<username>:<password>@cluster0.gefnhnd.mongodb.net/?retryWrites=true&w=majority";
    private const string CONNECTION_URL = "mongodb://<address>:<port>/?authSource=admin";

    private ILogger<DiscordBotWorker>? _logger = null;
    private IConfiguration? _config = null;

    private MongoClientSettings? _settings = null;
    private MongoClient? _client;
    private string _url = string.Empty;

    #endregion

    #region Properties

    /// <summary>
    /// Main database instance.
    /// </summary>
    public static MainDatabase Instance => s_instance;

    /// <summary>
    /// To check if the database connected to the source.
    /// </summary>
    public bool IsConnected => _client?.Cluster.Description.State == ClusterState.Connected;

    #endregion

    #region Constructor

    private MainDatabase() { }

    private MainDatabase(IConfiguration config)
    {
        // Replace formats.
        _url = CONNECTION_URL
            .Replace("<username>", config["MONGO_USER"])
            .Replace("<password>", config["MONGO_PW"])
            .Replace("<address>", config["ADDRESS"])
            .Replace("<port>", config["PORT"]);

        // Create database connection settings.
        _settings = MongoClientSettings.FromConnectionString(_url);
        _settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        //_settings.DirectConnection = true; // Local connection only
    }

    #endregion

    #region Main

    /// <summary>
    /// Adds a new guildID as a collection.
    /// </summary>
    /// <param name="guildID">Yet registered Guild ID</param>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task<IMongoCollection<BsonDocument>> InitCollection(ulong guildID, string databaseName)
    {
        // Creates a dummy database
        IMongoDatabase? database = null;
        IMongoCollection<BsonDocument>? collection = null;

        // Initialize databases
        await HandleDBProcess(async () => {
            // Init database
            database = _client.GetDatabase(databaseName);

            // Check if the collection has already exists, then abort process.
            var cols = await database.ListCollectionNames().ToListAsync();
            if (cols.Any(i => i == $"{guildID}"))
            {
                collection = database.GetCollection<BsonDocument>($"{guildID}");
                return;
            }

            // Initialize collection
            await database.CreateCollectionAsync($"{guildID}");
            collection = database.GetCollection<BsonDocument>($"{guildID}");
        });

        return collection;
    }

    /// <summary>
    /// Handle attempting to connect to the server.
    /// Run this when the database client is not connected.
    /// </summary>
    /// <returns>Newly connected client</returns>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    internal async Task<MongoClient> AttemptReconnect()
    {
        _client = new MongoClient(_settings);
        var tries = LIMIT_TRIES;

        while (!IsConnected && tries > 0)
        {
            _logger?.LogInformation("Connecting to database...");
            await Task.Delay(1000);
            tries--;

            if (tries == 0)
            {
                _logger?.LogInformation("Unable to connect! Connection will be postponed.");
                throw new DBClientTimeoutException();
            }
        }

        _logger?.LogInformation("Successfully connected!");
        return _client;
    }

    private async Task<List<string>> InitCollectionList()
    {
        IMongoDatabase? database = null;
        List<string> result = new();

        await HandleDBProcess(async () => {
            database = _client?.GetDatabase(DB_CONSTANT.INTERFACE_DATABASE_NAME);

            if (database != null)
                result = await database.ListCollectionNames().ToListAsync();
        });

        return result;
    }

    /// <summary>
    /// This will clear an unused data from database if the message is no more.
    /// </summary>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task DeleteAllUnusedMissingMessage(DiscordGuild guild)
    {
        FilterDefinition<BsonDocument> filterByChannel;
        BsonDocument messages, buttons;
        DiscordChannel channelObj;
        DiscordMessage messageObj;
        string? channelID;
        bool foundButton;

        await HandleDBProcess(async () => {
            // Initializing database.
            var client = await GetClient();
            var database = client.GetDatabase(DB_CONSTANT.INTERFACE_DATABASE_NAME);
            _logger?.LogInformation($"[DEBUG] Start deleting all the unused missing messages... (In Guild: {guild.Id})");

            // Get the data collection by guild ID.
            var collection = database.GetCollection<BsonDocument>($"{guild.Id}");
            var cursors = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            foreach (var doc in cursors)
            {
                channelID = doc[DB_CONSTANT.CHANNEL_ID_KEY].ToString();

                // Making sure the channel id is there.
                if (channelID == null) continue;
                filterByChannel = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.CHANNEL_ID_KEY, channelID);
                _logger?.LogInformation($"[DEBUG] Checking for channel with ID: {channelID} (In Guild: {guild.Id})");

                // Check if the channel in server has been deleted.
                channelObj = guild.GetChannel(ulong.Parse(channelID));
                if (channelObj == null)
                {
                    await collection.DeleteOneAsync(filterByChannel);
                    continue;
                }

                // Get all messages from database.
                messages = doc[DB_CONSTANT.MESSAGE_ID_KEY].AsBsonDocument;
                foreach (var m in messages.Elements.ToList())
                {
                    try
                    {
                        messageObj = await channelObj.GetMessageAsync(ulong.Parse(m.Name));
                    }
                    catch (NotFoundException)
                    {
                        // if the message has been deleted or not found.
                        messages.Remove(m.Name);
                        continue;
                    }
                    
                    buttons = m.Value.AsBsonDocument;

                    foreach (var btn in buttons.Elements.ToList())
                    {
                        // Check if the button component has been deleted.
                        foundButton = messageObj.Components.Any((component) => {
                            if (component.Type != ComponentType.Button) return false;
                            return component.CustomId == btn.Name;
                        });

                        if (!foundButton)
                            buttons.Remove(btn.Name);
                    }
                }
                _logger?.LogInformation($"[DEBUG] Cleaning up channel with ID: {channelID} (In Guild: {guild.Id})");

                if (messages.ElementCount == 0)
                    await collection.DeleteOneAsync(filterByChannel);
                else
                    await collection.ReplaceOneAsync(filterByChannel, doc);
            }
        });
    }

    public async Task<MongoClient> GetClient()
    {
        if (_client == null)
            _client = await AttemptReconnect();
        else if (!IsConnected)
            _client = await AttemptReconnect();

        return _client;
    }

    public static async Task Init(ILogger<DiscordBotWorker> logger, IConfiguration config)
    {
        if (s_instance != null) return;

        s_instance = new MainDatabase(config)
        {
            _logger = logger,
            _config = config,
        };

        await s_instance.AttemptReconnect();
    }

    /// <summary>
    /// Used to handle database process.
    /// </summary>
    /// <param name="func">Must contains database process.</param>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task HandleDBProcess(Func<Task> func)
    {
        var tries = LIMIT_TRIES;
        
        do {
            try
            {
                // Handle database process.
                await func();
            }
            catch (MongoConnectionException)
            {
                _logger?.LogError($"[ERROR] Connection failure, attempting to reconnect...");
                _client = await AttemptReconnect();
                tries--;
            }
            catch (TimeoutException)
            {
                _logger?.LogError($"[ERROR] Connection Timeout! Try again in few moments.");
                throw new DBClientTimeoutException();
            }
        } while (!IsConnected && tries > 0);
    }

    /// <summary>
    /// Used to handle database process.
    /// </summary>
    /// <param name="func">Must contains database process.</param>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task<T?> HandleDBProcess<T>(Func<Task<T>> func)
    {
        var tries = LIMIT_TRIES;
        
        do {
            try
            {
                // Handle database process.
                return await func();
            }
            catch (MongoConnectionException)
            {
                _logger?.LogError($"[ERROR] Connection failure, attempting to reconnect...");
                _client = await AttemptReconnect();
                tries--;
            }
            catch (TimeoutException)
            {
                _logger?.LogError($"[ERROR] Connection Timeout! Try again in few moments.");
                throw new DBClientTimeoutException();
            }
        } while (!IsConnected && tries > 0);

        return default(T);
    }

    #endregion
}
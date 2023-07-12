using DSharpPlus;
using DSharpPlus.Entities;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FurmAppDBot.Databases;

[Serializable]
public sealed class FormInterfaceData : DataElementBase, ILoadDatabase, ISaveDatabase
{
    #region structs

    [Serializable]
    private struct QuestionProperties
    {
        #region Variables

        public string question;
        public TextInputStyle style;
        public string placeholder;
        public bool required;
        public int minLength;
        public int maxLength;

        #endregion
    }

    #endregion

    #region Variables

    private List<QuestionProperties> _questions = new();

    private ulong _guildID = 0;
    private string _formID = string.Empty;

    #endregion

    #region Properties

    /// <summary>
    /// Get question information by question number.
    /// Question number starts from 0.
    /// </summary>
    public TextInputComponent this[int questionNumber] => new TextInputComponent(
        _questions[questionNumber].question, $"q{questionNumber}")
    {
        Style = _questions[questionNumber].style,
        Placeholder = _questions[questionNumber].placeholder,
        Required = _questions[questionNumber].required,
        MinimumLength = _questions[questionNumber].minLength,
        MaximumLength = _questions[questionNumber].maxLength,
    };

    public ulong GuildID => _guildID;

    public string FormID => _formID;

    public int QuestionCount => _questions.Count;

    #endregion

    #region Constructor

    public FormInterfaceData(MainDatabase database) : base(database) { }

    public FormInterfaceData(MainDatabase database, ulong guildID, string formID) : base(database)
    {
        _guildID = guildID;
        _formID = formID;
    }

    #endregion

    #region ILoadDatabase

    /// <para>
    /// WARNING: This will replace the questions data, do not run unless you have backed up the data.
    /// </para>
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public async Task<bool> LoadData()
    {
        await DatabaseRef.HandleDBProcess(async () => {
            // Init collection
            var collection = await DatabaseRef.InitCollection(_guildID, DB_CONSTANT.FORM_DATABASE_NAME);

            // Get data document from database with filter
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.FORM_ID_KEY, _formID);
            var documentFound = await collection.Find(filter).ToListAsync();

            // Check if document not found
            if (documentFound.Count == 0) return;

            // Clear old data
            _questions.Clear();

            // Receive all data
            var array = documentFound[0][DB_CONSTANT.FORM_QUESTIONS_KEY].AsBsonArray;
            foreach (var q in array)
            {
                _questions.Add(new QuestionProperties {
                    question = q["question"].AsString,
                    style = (TextInputStyle)q["style"].AsInt32,
                    placeholder = q["placeholder"].AsString,
                    required = q["required"].AsBoolean,
                    minLength = q["minLength"].AsInt32,
                    maxLength = q["maxLength"].AsInt32,
                });
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
            // Init collection
            var collection = await DatabaseRef.InitCollection(_guildID, DB_CONSTANT.FORM_DATABASE_NAME);

            // Get data document from database with filter
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.FORM_ID_KEY, _formID);
            var documentFound = await collection.Find(filter).ToListAsync();

            // Check unique data which don't exists in database.
            if (documentFound.Count == 0)
            {
                await collection.InsertOneAsync(new BsonDocument {
                    { DB_CONSTANT.FORM_ID_KEY, $"{_formID}" },
                    { DB_CONSTANT.FORM_QUESTIONS_KEY, new BsonArray() },
                });
            }

            // Update questions data.
            await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set(DB_CONSTANT.FORM_QUESTIONS_KEY, _questions));
        });

        return true;
    }

    #endregion

    #region Main

    public async override Task ConnectElement(DataElementBase with, params string[] keys)
    {
        if (with is ButtonInterfaceData)
        {
            // Convert to form interface data
            var btnData = (ButtonInterfaceData)with;

            // First index is the message ID, second index is the button ID.
            btnData.SetFormID(keys[0], keys[1], _formID);
            await btnData.SaveData();
            return;
        }

        throw new ConnectElementFailedException();
    }

    /// <summary>
    /// Add a question to form.
    /// Must run Save method to make sure it's saved to database.
    /// </summary>
    /// <param name="question">The question</param>
    /// <param name="style">Input style, is it short or paragraph</param>
    /// <param name="placeholder">Give an answer suggestion</param>
    /// <param name="required">Is this question must be filled by the user</param>
    /// <param name="minLength">User minimal input length</param>
    /// <param name="maxLength">User maximal input length</param>
    public void AddQuestion(string question, TextInputStyle style, string placeholder, bool required, int minLength, int maxLength)
    {
        // Making sure the question ID is unique.
        _questions.Add(new QuestionProperties {
            placeholder = placeholder,
            question = question,
            required = required,
            style = style,
            minLength = minLength,
            maxLength = maxLength,
        });
    }

    /// <summary>
    /// Remove a question from the form data.
    /// Must run Save method to make sure it's saved to database.
    /// </summary>
    /// <param name="questionNumber">Question number starts from 0</param>
    public void RemoveQuestion(int questionNumber)
    {
        var deletedContent = _questions[questionNumber];
        _questions.RemoveAt(questionNumber);
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task<bool> Exists(ulong guildID, string formID)
    {
        // Search via cache.
        var db = MainDatabase.Instance;
        var result = false;

        await db.HandleDBProcess(async () => {
            // Init collection.
            var collection = await db.InitCollection(guildID, DB_CONSTANT.FORM_DATABASE_NAME);

            // Find data from database collection using filter.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.FORM_ID_KEY, formID);
            var formFound = await collection.Find(filter).ToListAsync();

            // Check if not found, then it is valid.
            if (formFound.Count == 0) return;

            // Set result value.
            result = true;
        });

        return result;
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task<FormInterfaceData> GetData(ulong guildID, string formID)
    {
        // Immediately load data from database
        FormInterfaceData data = new FormInterfaceData(MainDatabase.Instance, guildID, formID);
        await data.LoadData();
        return data;
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task DeleteData()
    {
        await Task.CompletedTask;
    }

    #endregion
}

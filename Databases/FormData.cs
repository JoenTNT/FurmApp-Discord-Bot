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
public sealed class FormData : DataElementBase, ILoadDatabase, ISaveDatabase
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

    /// <summary>
    /// This channel name by default is empty.
    /// Admin/Mod can set this info to make sure which channel they want user's submission will be send.
    /// This is just a customization, by default user's submission will be send to channel with form ID as a name.
    /// WARNING: Same channel name with different form submission is possible, it is recommended to use form ID as channel name.
    /// </summary>
    private string _channelName = string.Empty;

    #endregion

    #region Properties

    /// <summary>
    /// Get question information by question number.
    /// Question number starts from 0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Index is out of range.
    /// </exception>
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

    /// <summary>
    /// This channel name by default is empty.
    /// Admin/Mod can set this info to make sure which channel they want user's submission will be send.
    /// This is just a customization, by default user's submission will be send to channel with form ID as a name.
    /// WARNING: Same channel name with different form submission is possible, it is recommended to use form ID as channel name.
    /// </summary>
    public string ChannelName => string.IsNullOrEmpty(_channelName) ? _formID : _channelName;

    #endregion

    #region Constructor

    public FormData(MainDatabase database) : base(database) { }

    public FormData(MainDatabase database, ulong guildID, string formID) : base(database)
    {
        _guildID = guildID;
        _formID = formID;
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
        if (with is InterfaceData)
        {
            // Convert to form interface data
            var btnData = (InterfaceData)with;

            // First index is the message ID, second index is the button ID.
            btnData.SetButtonFormID(keys[0], keys[1], _formID);
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
    /// <param name="index">Question index starts from 0</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Index is out of range.
    /// </exception>
    public void RemoveQuestion(int index) => _questions.RemoveAt(index);

    /// <summary>
    /// Move question to target index.
    /// </summary>
    /// <param name="fromIndex">The target question</param>
    /// <param name="toIndex">Move to this index</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Index is out of range.
    /// </exception>
    public void MoveQuestion(int fromIndex, int toIndex)
    {
        // If the index is the same value, then no need to process.
        if (fromIndex == toIndex) return;

        // Shifting questions.
        var temp = _questions[fromIndex];
        _questions.RemoveAt(fromIndex);
        _questions.Insert(toIndex, temp);
    }

    /// <summary>
    /// Set question properties by question number.
    /// </summary>
    /// <param name="questionNum">Question number starts from 0.</param>
    /// <param name="qText">Question text content</param>
    /// <param name="style">Text component style</param>
    /// <param name="placeholder">Placeholder hint for user</param>
    /// <param name="req">Is the question required for user to input?</param>
    /// <param name="min">Minimal length of letters user must answer</param>
    /// <param name="max">Maximal length of letters user must answer</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Index is out of range.
    /// </exception>
    public void SetQuestionProps(int questionNum, string? qText = null, TextInputStyle? style = null,
        string? placeholder = null, bool? req = null, int? min = null, int? max = null)
    {
        // Get property information.
        QuestionProperties prop = _questions[questionNum];

        // Check any properties that will be changed.
        if (qText != null) prop.question = qText;
        if (style != null) prop.style = style == TextInputStyle.Short ? TextInputStyle.Short : TextInputStyle.Paragraph;
        if (placeholder != null) prop.placeholder = placeholder;
        if (req != null) prop.required = req == true ? true : false;
        if (min != null) prop.minLength = (int)min;
        if (max != null) prop.maxLength = (int)max;

        // Set back to source.
        _questions[questionNum] = prop;
    }

    /// <summary>
    /// Swap between 2 question position.
    /// </summary>
    /// <param name="q1Index">Question 1 index</param>
    /// <param name="q2Index">Question 2 index</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Index is out of range.
    /// </exception>
    public void SwapQuestion(int q1Index, int q2Index)
    {
        // If the index is the same value, then no need to process.
        if (q1Index == q2Index) return;

        // Swapping 2 questions.
        var temp = _questions[q1Index];
        _questions[q1Index] = _questions[q2Index];
        _questions[q2Index] = temp;
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    private static async Task<bool> Exists(ulong guildID, string formID)
    {
        // Search via cache.
        var db = MainDatabase.Instance;
        var result = false;

        await db.HandleDBProcess(async () => {
            // Init collection.
            var collection = await db.InitCollection(guildID, DB_CONSTANT.FORM_DATABASE_NAME);

            // Find data from database collection using filter.
            var filter = Builders<BsonDocument>.Filter.Eq(DB_CONSTANT.FORM_ID_KEY, formID);
            result = await collection.Find(filter).AnyAsync();
        });

        return result;
    }

    
    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    /// <exception cref="FormNotFoundException">
    /// Specific case if form does not exists from database.
    /// </exception>
    public static async Task<FormData> GetData(ulong guildID, string formID)
    {
        // Check if form exists.
        if (!(await Exists(guildID, formID)))
            throw new FormNotFoundException(guildID, formID);

        // Immediately load data from database
        FormData data = new FormData(MainDatabase.Instance, guildID, formID);
        await data.LoadData();
        return data;
    }

    /// <exception cref="DBClientTimeoutException">
    /// When database client connection timeout happens.
    /// </exception>
    public static async Task<FormData> CreateData(ulong guildID, string formID)
    {
        // Declare data.
        FormData data;

        // Check if form exists.
        if (await Exists(guildID, formID))
        {
            data = new FormData(MainDatabase.Instance, guildID, formID);
            await data.LoadData();
            return data;
        }
        
        // Immediately create data and save it to database.
        data = new FormData(MainDatabase.Instance, guildID, formID);
        await data.SaveData();
        return data;
    }

    #endregion
}

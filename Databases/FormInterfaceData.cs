using DSharpPlus;
using DSharpPlus.Entities;

namespace FurmAppDBot.Databases;

[Serializable]
public sealed class FormInterfaceData : DataElementBase, ILoadDatabase, ISaveDatabase
{
    #region Variables

    private List<TextInputComponent> _questions = new();

    private string _guildID = string.Empty;
    private string _formID = string.Empty;

    #endregion

    #region Properties

    /// <summary>
    /// Get question information by question number.
    /// Question number starts from 0.
    /// </summary>
    public TextInputComponent this[int questionNumber] => _questions[questionNumber];

    public string FormID
    {
        get => _formID;
        set => _formID = value;
    }

    public int QuestionCount => _questions.Count;

    #endregion

    #region Constructor

    public FormInterfaceData(MainDatabase database) : base(database) { }

    public FormInterfaceData(MainDatabase database, string formID) : base(database)
    {
        _formID = formID;
        
    }

    #endregion

    #region ILoadDatabase

    public async Task<bool> LoadData()
    {
        return true;
    }

    #endregion

    #region ISaveDatabase

    public async Task<bool> SaveData()
    {
        return true;
    }

    #endregion

    #region Main

    public void AddQuestion(string question, TextInputStyle style, string placeholder, bool required)
    {
        var newQuestion = new TextInputComponent(question, $"q{_questions.Count}") {
            Placeholder = placeholder,
            Required = required,
            Style = style,
        };

        _questions.Add(newQuestion);
    }

    /// <summary>
    /// Remove a question from the form data.
    /// </summary>
    /// <param name="questionNumber">Question number starts from 0</param>
    public void RemoveQuestion(int questionNumber) => _questions.RemoveAt(questionNumber);

    public static async Task<FormInterfaceData> GetData(ulong guildID)
    {

    }

    #endregion
}

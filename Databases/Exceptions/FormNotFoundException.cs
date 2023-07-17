namespace FurmAppDBot.Databases.Exceptions;

public class FormNotFoundException : Exception
{
    #region Variables

    private string _formID = string.Empty;
    private ulong _guildID = 0;

    #endregion

    #region Properties

    public string FormID => _formID;
    public ulong GuildID => _guildID;

    #endregion

    public FormNotFoundException(ulong guildID, string formID)
    {
        _guildID = guildID;
        _formID = formID;
    }
}
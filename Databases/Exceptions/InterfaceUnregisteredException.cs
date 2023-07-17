namespace FurmAppDBot.Databases.Exceptions;

public class InterfaceUnregisteredException : Exception
{
    #region Variables

    private ulong _guildID = 0;

    private ulong _channelID = 0;

    #endregion

    #region Properties

    public ulong GuildID => _guildID;

    public ulong ChannelID => _channelID;

    #endregion

    #region Constructor

    public InterfaceUnregisteredException(ulong guildID, ulong channelID)
    {
        _guildID = guildID;
        _channelID = channelID;
    }

    #endregion
}
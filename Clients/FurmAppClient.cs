#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604

using DSharpPlus;

namespace FurmAppDBot.Clients;

public class FurmAppClient
{
    #region enum

    /// <summary>
    /// Debug type for logging.
    /// </summary>
    public enum DebugType : int
    { 
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    #endregion

    #region Variables

    private static FurmAppClient? s_instance = null;

    private DiscordClient? _client = null;
    private ILogger<Worker>? _logger = null;
    private IConfiguration? _config = null;

    #endregion

    #region Properties

    public static FurmAppClient Instance => s_instance;

    public DiscordClient Client => _client;

    public ILogger<Worker> Logger => _logger;

    public IConfiguration Config => _config;

    #endregion

    #region Constructor

    private FurmAppClient() { }

    #endregion

    #region Main

    public static void Init(DiscordClient client, ILogger<Worker> logger, IConfiguration config)
    {
        if (s_instance != null) return;

        s_instance = new FurmAppClient();
        s_instance._client = client;
        s_instance._logger = logger;
        s_instance._config = config;
    }

    public static void Debug(string? message, DebugType type = DebugType.Info)
    {
        switch (type)
        {
            case DebugType.Info:
                s_instance._logger.LogInformation(message);
                break;
            case DebugType.Warning:
                s_instance._logger.LogWarning(message);
                break;
            case DebugType.Error:
                s_instance._logger.LogError(message);
                break;
        }
    }

    #endregion
}
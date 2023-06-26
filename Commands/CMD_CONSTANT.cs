namespace FurmAppDBot.Commands;

/// <summary>
/// Only for commands, constants properties used for this bot.
/// </summary>
public static class CMD_CONSTANT
{
    /// <summary>
    /// Timeout for each asynchronous waiter for next response.
    /// </summary>
    public const int TIMEOUT_SECONDS_DEFAULT = 60; // Means 60 seconds before timeout

    public const string EMBED_HEX_COLOR_DEFAULT = "FFFFFF"; // White color embed


    public const string PING_COMMAND_NAME = "ping";

    public const string PING_COMMAND_DESCRIPTION = "Replies with Pong!";

    /// <summary>
    /// Help command name.
    /// </summary>
    public const string HELP_COMMAND_NAME = "help";

    /// <summary>
    /// Help command description.
    /// </summary>
    public const string HELP_COMMAND_DESCRIPTION = "Provides a command guide, helps, and quick start.";

    public const string EMBED_COMMAND_NAME = "embed";

    public const string EMBED_COMMAND_DESCRIPTION = "Create Embed Message.";

    public const string GET_PREFIX_COMMAND_NAME = "getprefix";

    public const string GET_PREFIX_COMMAND_DESCRIPTION = "Get prefix of bot command in this server.";

    public const string SET_BUTTON_COMMAND_NAME = "setbutton";

    public const string SET_BUTTON_COMMAND_DESCRIPTION = "Set button on target message.";

    public const string GET_BUTTON_COMMAND_NAME = "getbutton";

    public const string GET_BUTTON_COMMAND_DESCRIPTION = "Get button information on target message.";

    public const string REMOVE_BUTTON_COMMAND_NAME = "delbutton";

    public const string REMOVE_BUTTON_COMMAND_DESCRIPTION = "Delete a button interface from the message, Button ID must be specific";

    public const string ADD_FORM_COMMAND_NAME = "addform";

    public const string ADD_FORM_COMMAND_DESCRIPTION = "Add a new form.";

    public const string GET_ALL_FORM_COMMAND_NAME = "getallform";

    public const string GET_ALL_FORM_COMMAND_DESCRIPTION = "Get all created form information from this server.";

    public const string PURGE_COMMAND_NAME = "purge";

    public const string PURGE_COMMAND_DESCRIPTION = "Purge/Delete messages in this channel bottom-up, the command is uncounted.";
}
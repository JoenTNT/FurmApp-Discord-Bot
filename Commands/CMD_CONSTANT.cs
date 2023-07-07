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

    // Main commands
    public const string PING_COMMAND_NAME = "ping";
    public const string HELP_COMMAND_NAME = "help";
    public const string EMBED_COMMAND_NAME = "embed";
    public const string GET_PREFIX_COMMAND_NAME = "getprefix";
    public const string BUTTON_COMMAND_NAME = "button";
    public const string FORM_COMMAND_NAME = "form";
    public const string ADD_COMMAND_NAME = "add";
    public const string GET_COMMAND_NAME = "get";
    public const string GET_ALL_COMMAND_NAME = "getall";
    public const string DELETE_COMMAND_NAME = "delete";
    public const string CONNECT_COMMAND_NAME = "connect";
    public const string PURGE_COMMAND_NAME = "purge";
    internal const string SYNCDB_COMMAND_NAME = "syncdb";

    // Command descriptions
    public const string PING_COMMAND_DESCRIPTION = "Replies with Pong!";
    public const string HELP_COMMAND_DESCRIPTION = "Provides a command guide, helps, and quick start.";
    public const string EMBED_COMMAND_DESCRIPTION = "Create Embed Message.";
    public const string BUTTON_COMMAND_DESCRIPTION = "Modifying button interface on target message.";
    public const string FORM_COMMAND_DESCRIPTION = "Utilizing modala to make a survey utility.";
    public const string GET_PREFIX_COMMAND_DESCRIPTION = "Get prefix of bot command in this server.";
    public const string BUTTON_ADD_COMMAND_DESCRIPTION = "Add button interface on target message.";
    public const string BUTTON_GET_COMMAND_DESCRIPTION = "Get button information on target message.";
    public const string BUTTON_DELETE_COMMAND_DESCRIPTION = "Delete a button interface from the message, Button ID must be specific";
    public const string ADD_FORM_COMMAND_DESCRIPTION = "Add a new form.";
    public const string GET_FORMS_COMMAND_DESCRIPTION = "Get all created form information from this server.";
    public const string CONNECT_COMMAND_DESCRIPTION = "Connect the component to form with IDs.";
    public const string PURGE_COMMAND_DESCRIPTION = "Purge/Delete messages in this channel bottom-up, the command is uncounted.";

    // Sub commands
    public const string CREATE_COMMAND_NAME = "create";
    public const string EDIT_COMMAND_NAME = "edit";
    public const string EMBED_CREATE_DESCRIPTION = "Create an embed.";
    public const string EMBED_EDIT_DESCRIPTION = "Edit one element of embed.";

    // Parameters
    public const string AMOUNT_PARAMETER = "amount";
    public const string FORM_ID_PARAMETER = "formid";
    public const string FORM_ID_PARAMETER_DESCRIPTION = "Target form ID that has been made.";
    public const string PURGE_AMOUNT_PARAMETER_DESCRIPTION = "How many messages will be deleted before calling this command. For example: 10";
    public const string EMBED_AUTHOR_ICON_URL_PARAMETER = "authoriconurl";
    public const string EMBED_AUTHOR_NAME_PARAMETER = "authorname";
    public const string EMBED_COLOR_PARAMETER = "embedcolor";
    public const string EMBED_DESCRIPTION_PARAMETER = "description";
    public const string EMBED_FOOTER_ICON_URL_PARAMETER = "footericonurl";
    public const string EMBED_FOOTER_TEXT_PARAMETER = "footertext";
    public const string EMBED_IMAGE_URL_PARAMETER = "imageurl";
    public const string EMBED_THUMBNAIL_URL_PARAMETER = "thumbnailurl";
    public const string EMBED_TITLE_PARAMETER = "title";
    public const string MESSAGE_ID_PARAMETER = "messageid";
    public const string MESSAGE_ID_PARAMETER_DESCRIPTION = "Target message ID, For Example: 1234567890123456789";
    public const string BUTTON_ID_PARAMETER = "buttonid";
    public const string BUTTON_ID_PARAMETER_DESCRIPTION = "Target button ID, each button has unique ID";
}
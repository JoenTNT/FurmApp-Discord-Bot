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
    public const string PREFIX_COMMAND_NAME = "prefix";
    public const string SETTING_COMMAND_NAME = "setting";
    public const string BUTTON_COMMAND_NAME = "button";
    public const string FORM_COMMAND_NAME = "form";
    public const string CONTAINER_COMMAND_NAME = "container";
    public const string QUESTION_COMMAND_NAME = "qt";
    public const string CREATE_COMMAND_NAME = "create";
    public const string ADD_COMMAND_NAME = "add";
    public const string GET_COMMAND_NAME = "get";
    public const string GET_ALL_COMMAND_NAME = "getall";
    public const string DELETE_COMMAND_NAME = "delete";
    public const string CONNECT_COMMAND_NAME = "connect";
    public const string PURGE_COMMAND_NAME = "purge";
    public const string EDIT_COMMAND_NAME = "edit";
    public const string SET_COMMAND_NAME = "set";
    public const string SWAP_COMMAND_NAME = "swap";
    public const string DUPLICATE_COMMAND_NAME = "dupe";
    public const string RENAME_COMMAND_NAME = "rename";
    public const string CATEGORY_COMMAND_NAME = "chcat";
    public const string DM_CHANNEL_COMMAND_NAME = "chdm";
    public const string ENABLE_COMMAND_NAME = "enable";
    public const string DISABLE_COMMAND_NAME = "disable";
    public const string INFO_COMMAND_NAME = "info";
    internal const string SYNCDB_COMMAND_NAME = "syncdb";

    // Command descriptions
    public const string PING_COMMAND_DESCRIPTION = "Replies with Pong!";
    public const string HELP_COMMAND_DESCRIPTION = "Provides a command guide, helps, and quick start.";
    public const string EMBED_COMMAND_DESCRIPTION = "Create Embed Message.";
    public const string EMBED_CREATE_COMMAND_DESCRIPTION = "Create an embed.";
    public const string EMBED_EDIT_COMMAND_DESCRIPTION = "Edit one element of embed.";
    public const string PREFIX_COMMAND_DESCRIPTION = "Prefix settings for this server.";
    public const string SETTING_COMMAND_DESCRIPTION = "Server scope settings";
    public const string BUTTON_COMMAND_DESCRIPTION = "Modifying button interface on target message.";
    public const string FORM_COMMAND_DESCRIPTION = "Utilizing form modal to make a survey utility.";
    public const string DUPLICATE_FORM_COMMAND_DESCRIPTION = "Duplicate existing form to new form with different ID.";
    public const string RENAME_FORM_COMMAND_DESCRIPTION = "Rename existing form with new ID.";
    public const string CHANNEL_CONTAINER_COMMAND_DESCRIPTION = "If user submit form answer, where it should be send?";
    public const string CC_SET_COMMAND_DESCRIPTION = "Name a channel which user submmision will be send to.";
    public const string QUESTION_COMMAND_DESCRIPTION = "Modify questions in one target form.";
    // public const string GET_PREFIX_COMMAND_DESCRIPTION = "Get prefix of bot command in this server."; // TODO: Prefix command.
    public const string BUTTON_ADD_COMMAND_DESCRIPTION = "Add button interface on target message.";
    public const string BUTTON_GET_COMMAND_DESCRIPTION = "Get button information on target message.";
    public const string BUTTON_DELETE_COMMAND_DESCRIPTION = "Delete a button interface from the message, Button ID must be specific";
    public const string CREATE_FORM_COMMAND_DESCRIPTION = "Create or add a new server's form.";
    public const string DELETE_FORM_COMMAND_DESCRIPTION = "Delete existing form.";
    public const string GET_FORM_DETAIL_COMMAND_DESCRIPTION = "Show form detail information by ID.";
    public const string GET_FORMS_COMMAND_DESCRIPTION = "Get all created form information from this server.";
    public const string CONNECT_COMMAND_DESCRIPTION = "Connect the component to form with IDs.";
    public const string QUESTION_ADD_COMMAND_DESCRIPTION = "Add question to target form.";
    public const string QUESTION_DELETE_COMMAND_DESCRIPTION = "Delete question from target form.";
    public const string QUESTION_EDIT_COMMAND_DESCRIPTION = "Edit question from targetr form.";
    public const string SETTING_CATEGORY_COMMAND_DESCRIPTION = "This command will set channel category as a submission container.";
    public const string SETTING_INFO_COMMAND_DESCRIPTION = "Check server scope settings information.";
    public const string PURGE_COMMAND_DESCRIPTION = "Purge/Delete messages in this channel bottom-up, the command is uncounted.";

    // Parameters
    public const string AMOUNT_PARAMETER = "amount";
    public const string COMMAND_NAME_PARAMETER = "command";
    public const string FORM_ID_PARAMETER = "formid";
    public const string CHANNEL_ID_PARAMETER = "channelid";
    public const string EMBED_AUTHOR_ICON_URL_PARAMETER = "authoriconurl";
    public const string EMBED_AUTHOR_NAME_PARAMETER = "authorname";
    public const string EMBED_COLOR_PARAMETER = "embedcolor";
    public const string EMBED_DESCRIPTION_PARAMETER = "description";
    public const string EMBED_FOOTER_ICON_URL_PARAMETER = "footericonurl";
    public const string EMBED_FOOTER_TEXT_PARAMETER = "footertext";
    public const string EMBED_IMAGE_URL_PARAMETER = "imageurl";
    public const string EMBED_THUMBNAIL_URL_PARAMETER = "thumbnailurl";
    public const string EMBED_TITLE_PARAMETER = "title";
    public const string ELEMENT_PARATEMER = "element";
    public const string VALUE_PARAMETER = "value";
    public const string MESSAGE_ID_PARAMETER = "messageid";
    public const string BUTTON_ID_PARAMETER = "buttonid";
    public const string CHANNEL_CATEGORY_ID_PARAMETER = "chcatid";
    public const string QUESTION_TEXT_PARAMETER = "question";
    public const string QUESTION_INPUT_STYLE_PARAMETER = "style";
    public const string QUESTION_PLACEHOLDER_PARAMETER = "placeholder";
    public const string QUESTION_REQUIRED_PARAMETER = "required";
    public const string QUESTION_MIN_PARAMETER = "min";
    public const string QUESTION_MAX_PARAMETER = "max";
    public const string QUESTION_NUMBER_PARAMETER = "qnum";

    // Parameter descriptions
    public const string HELP_COMMAND_NAME_PARAMETER_DESCRIPTION = "Name the command if you need a specific help.";
    public const string FORM_ID_PARAMETER_DESCRIPTION = "Target form ID that has been made.";
    public const string PURGE_AMOUNT_PARAMETER_DESCRIPTION = "How many messages will be deleted before calling this command. For example: 10";
    public const string CHANNEL_ID_PARAMETER_DESCRIPTION = "What is the channel ID target? For example: 1234567890123456789";
    public const string MESSAGE_ID_PARAMETER_DESCRIPTION = "Target message ID, For example: 1234567890123456789";
    public const string BUTTON_ID_PARAMETER_DESCRIPTION = "Target button ID, each button has unique ID";
    public const string CHANNEL_CATEGORY_ID_PARAMETER_DESCRIPTION = "Insert channel category ID, For example: 1234567890123456789";
    public const string EMBED_ELEMENT_PARAMETER_DESCRIPTION = "Target element in that embed.";
    public const string EMBED_VALUE_PARAMETER_DESCRIPTION = "Set value of the element, value may be vary and may not always work.";
    public const string QUESTION_TEXT_PARAMETER_DESCRIPTION = "The question itself. For example: \"What is your Favourite Music?\"";
    public const string QUESTION_PLACEHOLDER_PARAMETER_DESCRIPTION = "A hint to help user answering a question. For example: \"Insert Genre or Song Title here...\"";
    public const string QUESTION_STYLE_PARAMETER_DESCRIPTION = "Input field style, paragraph or short?";
    public const string QUESTION_REQUIRED_PARAMETER_DESCRIPTION = "Does user must answer the question?";
    public const string QUESTION_MIN_PARAMETER_DESCRIPTION = "Minimal length of letter that user must input. Default is 1.";
    public const string QUESTION_MAX_PARAMETER_DESCRIPTION = "Maximal length of letter that user can input. Default is 512.";
    public const string QUESTION_NUMBER_PARAMETER_DESCRIPTION = "Question number, always starts from number 1 to n.";
}
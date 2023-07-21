namespace FurmAppDBot.Databases;

public enum InterfaceType { None = 0, Button, }

public static class DB_CONSTANT
{
    public const string INTERFACE_DATABASE_NAME = "interface";
    public const string FORM_DATABASE_NAME = "form";
    public const string SUBMISSION_DATABASE_NAME = "submission";
    public const string BACKUP_DATABASE_NAME = "backup";
    public const string SETTING_DATABASE_NAME = "setting";
    
    public const string CHANNEL_ID_KEY = "channelID";
    public const string MESSAGE_ID_KEY = "messageID";
    public const string BUTTONS_KEY = "buttons";
    public const string FORM_ID_KEY = "formID";
    public const string FORM_QUESTIONS_KEY = "questions";
    public const string CHANNEL_AND_CATEGORY_AS_CONTAINER_KEY = "cc";

    public const string QUESTION_KEY = "q";
    public const string QUESTION_STYLE_KEY = "style";
    public const string QUESTION_PLACEHOLDER_KEY = "placeholder";
    public const string QUESTION_REQUIRED_KEY = "req";
}
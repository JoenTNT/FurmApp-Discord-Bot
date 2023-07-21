namespace FurmAppDBot.Databases;

[Flags]
public enum SendingSubmissionType : short
{
    ViaChannel = 1,
    ViaDM = 2,
}
namespace FurmAppDBot.Utilities;

public static class SimpleUtility
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string RandomStr(int length)
    {
        var rand = new Random();
        string result = string.Empty;

        for (int i = 0; i < length; i++)
            result += $"{CHARS[rand.Next(CHARS.Length)]}";

        return result;
    }

    /// <summary>
    /// This will unpack string for each properties.
    /// </summary>
    /// <param name="input">Template: key=value</param>
    /// <returns>Value</returns>
    public static string? GetValueAtFirstEqual(string input)
    {
        int startAt = input.IndexOf('=');
        return input.Substring(startAt + 1);
    }
}
using System.Text;

namespace FurmAppDBot.Utilities;

public static class SimpleUtility
{
    #region Variables

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    #endregion

    #region Statics

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

    /// <summary>
    /// Turn all strings into discord channel name.
    /// </summary>
    /// <param name="input">String to be converted.</param>
    /// <returns>Limited channel name result</returns>
    public static string ToDiscordChannelName(string input)
    {
        // Make string builder, convert all upper letters to lower.
        StringBuilder build = new StringBuilder();

        // Case 0: Always lower case.
        input = input.ToLower();

        // Case 1: No White Spaces, replace it with '-'.
        input = input.Replace(' ', '-');

        // Case 2: Cannot add '-' on the begining of the channel name.
        if (input[0] == '-') input = input.Remove(0, 1);

        // Case 3: Cannot use other special characters
        for (int i = 0; i < input.Length; i++)
        {
            // Check letter and digit, except for '-' or '_'.
            if (!Char.IsLetterOrDigit(input[i]) && !(input[i] == '-' || input[i] == '_'))
            {
                // Case 4: Must not have more than one '-'.
                if (build[build.Length - 1] == '-') continue;

                // Replace with '-'.
                build.Append('-');
                continue;
            }
            
            // Automatically add string if passed the checker.
            build.Append(input[i]);
        }

        // Return result value.
        return build.ToString();
    }

    #endregion
}
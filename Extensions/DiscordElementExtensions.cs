using DSharpPlus.Entities;

namespace FurmAppDBot.Extensions;

public static class DiscordElementExtensions
{
    /// <summary>
    /// To check if the component with ID is already exists.
    /// </summary>
    /// <param name="targetMsg">The message to be checked</param>
    /// <param name="componentID">Target ID if the message contains it</param>
    /// <returns>True if the message has a component with the target ID</returns>
    public static bool IsComponentWithIDExists(this DiscordMessage targetMsg, string componentID)
    {
        foreach (var comp in targetMsg.Components)
            foreach (var c in comp.Components)
                if (c.CustomId == componentID)
                    return true;

        return false;
    }
}

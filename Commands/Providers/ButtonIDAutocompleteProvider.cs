using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands.Providers;

public class ButtonIDAutocompleteProvider : IAutocompleteProvider
{
    #region  IAutocompleteProvider

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        // Find message ID, if not then return empty.
        var empty = Task.FromResult(new List<DiscordAutoCompleteChoice>().AsEnumerable());
        var msgIDOP = ctx.Options.FirstOrDefault(op => op.Name == CMD_CONSTANT.MESSAGE_ID_PARAMETER);
        if (msgIDOP == null) return empty;
        if (msgIDOP.Value is not string) return empty;
        DiscordMessage targetMsg;
        try
        {
            var task = ctx.Channel.GetMessageAsync(ulong.Parse((string)msgIDOP.Value));
            task.Wait();
            targetMsg = task.Result;
        }
        catch (Exception) { return empty; }

        // Return button ID result.
        return Task.FromResult(GetButtonIDs(targetMsg));
    }

    #endregion

    #region Main

    public IEnumerable<DiscordAutoCompleteChoice> GetButtonIDs(DiscordMessage targetMsg)
    {
        // Declare options provider.
        var l = new List<DiscordAutoCompleteChoice>();

        // Get all button IDs.
        foreach (var comp in targetMsg.Components)
        {
            foreach (var btn in comp.Components.OfType<DiscordButtonComponent>())
                l.Add(new DiscordAutoCompleteChoice($"{(string.IsNullOrEmpty(btn.Label) ? "NO LABEL" : btn.Label)} "
                    + $"[ID: {btn.CustomId}]", btn.CustomId));
        }

        // Return provider result.
        return l;
    }

    #endregion
}
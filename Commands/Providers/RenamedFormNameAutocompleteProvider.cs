using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Utilities;

namespace FurmAppDBot.Commands.Providers;

public class RenamedFormNameAutocompleteProvider : IAutocompleteProvider
{
    #region IAutocompleteProvider

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        return RenameForm(ctx.Guild, (string)ctx.FocusedOption.Value);
    }

    #endregion

    #region  Main

    private async Task<IEnumerable<DiscordAutoCompleteChoice>> RenameForm(DiscordGuild guild, string input)
    {
        // Change input.
        string formName = SimpleUtility.ToDiscordChannelName(input);

        // Check availability of form ID.
        var db = MainDatabase.Instance;
        bool isIDAvailable = await db.HandleDBProcess<bool>(async () => {
            try { await FormData.GetData(guild.Id, input); }
            catch (FormNotFoundException) { return false; }
            return true;
        });

        // Return hint information.
        return new DiscordAutoCompleteChoice[] { new DiscordAutoCompleteChoice(
            $"Renamed to \"{formName}\" [{(isIDAvailable ? "Available" : "Already Registered")}]", formName) };
    }

    #endregion
}
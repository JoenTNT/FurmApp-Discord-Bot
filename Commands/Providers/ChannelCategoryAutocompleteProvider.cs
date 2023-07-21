using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands.Providers;

public class ChannelCategoryAutocompleteProvider : IAutocompleteProvider
{
    #region IAutocompleteProvider

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        return GetAllChannelCategory(ctx.Guild, (string)ctx.FocusedOption.Value);
    }

    #endregion

    #region Main

    public async Task<IEnumerable<DiscordAutoCompleteChoice>> GetAllChannelCategory(DiscordGuild guild, string preInput)
    {
        // Declare options provider.
        var l = new List<DiscordAutoCompleteChoice>();

        // Get all channels.
        var channels = await guild.GetChannelsAsync();
        foreach (var ch in channels)
        {
            // Check if it is not category, then skip it.
            if (!ch.IsCategory) continue;

            // Check if the name is match with pre input, then add to option.
            if (ch.Name.Contains(preInput, StringComparison.OrdinalIgnoreCase))
            {
                l.Add(new DiscordAutoCompleteChoice(ch.Name, $"{ch.Id}"));
                continue;
            }

            // Check if pre input is an ID instead.
            if ($"{ch.Id}".Contains(preInput, StringComparison.OrdinalIgnoreCase))
                l.Add(new DiscordAutoCompleteChoice(ch.Name, $"{ch.Id}"));
        }

        // Return provider result.
        return l;
    }

    #endregion
}
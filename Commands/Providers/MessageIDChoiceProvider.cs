using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands.Providers;

public class MessageIDChoiceProvider : IChoiceProvider
{
    #region IChoiceProvider

    public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        return Task.FromResult(new List<DiscordApplicationCommandOptionChoice>().AsEnumerable());
    }

    #endregion

    #region Main

    private async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> GetOptionChoicesFromDB()
    {
        List<DiscordApplicationCommandOptionChoice> choices = new();

        try
        {
            await MainDatabase.Instance.HandleDBProcess(async () => {
                var client = await MainDatabase.Instance.GetClient();
            });
        }
        catch (DBClientTimeoutException)
        {

        }
        
        return choices.AsEnumerable();
    }

    #endregion
}

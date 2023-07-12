using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands.Providers;

public class MessageIDChoiceProvider : ChoiceProvider
{
    #region ChoiceProvider

    public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        //var context = Services.GetService<InteractionContext>();
        return Task.FromResult(new List<DiscordApplicationCommandOptionChoice>().AsEnumerable());
    }

    #endregion

    #region Main

    // TODO: Get message ID from database by guild and channel.
    // ISSUE: Cannot receive guild information.
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

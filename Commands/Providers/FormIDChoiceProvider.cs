using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Databases;

namespace  FurmAppDBot.Commands.Providers;

public class FormIDChoiceProvider : ChoiceProvider
{
    #region ChoiceProvider

    public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider() => GetAllForms();

    #endregion

    #region Main

    // TODO: Get all form ID from database by guild.
    // ISSUE: Cannot receive guild information.
    private async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> GetAllForms()
    {
        // Receive database instance.
        MainDatabase db = MainDatabase.Instance;

        // Declare options provider.
        var l = new List<DiscordApplicationCommandOptionChoice>();

        // Handle database process which timeout may occur.
        await db.HandleDBProcess(async () => {
            //db.InitCollection()
        });
        
        // Return provider result.
        return l;
    }

    #endregion
}
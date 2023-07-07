using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands.Providers;

public class CommandNameChoiceProvider : ChoiceProvider
{
    #region IChoiceProvider

    public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        return Task.FromResult(GetCommands());
    }

    #endregion

    #region Main

    private IEnumerable<DiscordApplicationCommandOptionChoice> GetCommands()
    {
        var l = new List<DiscordApplicationCommandOptionChoice> {
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.EMBED_COMMAND_NAME, CMD_CONSTANT.EMBED_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.BUTTON_COMMAND_NAME, CMD_CONSTANT.BUTTON_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.FORM_COMMAND_NAME, CMD_CONSTANT.FORM_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME, CMD_CONSTANT.GET_PREFIX_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.HELP_COMMAND_NAME, CMD_CONSTANT.HELP_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.PING_COMMAND_NAME, CMD_CONSTANT.PING_COMMAND_NAME),
            new DiscordApplicationCommandOptionChoice(CMD_CONSTANT.PURGE_COMMAND_NAME, CMD_CONSTANT.PURGE_COMMAND_NAME),
        };

        return l;
    }

    #endregion
}

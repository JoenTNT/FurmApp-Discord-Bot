using DSharpPlus;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.CHANNEL_CONTAINER_COMMAND_NAME, CMD_CONSTANT.CHANNEL_CONTAINER_COMMAND_DESCRIPTION)]
public class ContainerSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.SET_COMMAND_NAME, CMD_CONSTANT.CC_SET_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Set(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID,
        [Option(CMD_CONSTANT.CHANNEL_NAME_PARAMETER, CMD_CONSTANT.CHANNEL_NAME_PARAMETER_DESCRIPTION)]
        [ChannelTypes]
        string channelName)
    {
        // TODO: Set form container.
    }
}

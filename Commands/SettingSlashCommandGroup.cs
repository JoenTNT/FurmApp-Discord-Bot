using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.SETTING_COMMAND_NAME, CMD_CONSTANT.SETTING_COMMAND_DESCRIPTION)]
public class SettingSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.CATEGORY_COMMAND_NAME, CMD_CONSTANT.SETTING_CATEGORY_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Category(InteractionContext ctx)
    {
        // TODO: Settings category.
    }

    [SlashCommand(CMD_CONSTANT.INFO_COMMAND_NAME, CMD_CONSTANT.SETTING_INFO_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Info(InteractionContext ctx)
    {
        // TODO: Settings info.
    }
}
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public class HelpCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.HELP_COMMAND_NAME)]
    public async Task Help(CommandContext ctx, string? commandName = null)
    {
        // Intialize message handler.
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        // Start by choosing command.
        await ChooseCommand(ctx.User, msgHandler, commandName);
    }

    #endregion

    #region Statics

    public static async Task ChooseCommand(DiscordUser user, DiscordMessage msgHandler, string? commandName = null)
    {
        do {
            // Check any command for help.
            if (string.IsNullOrEmpty(commandName))
            {
                commandName = await Main(user, msgHandler);
                return;
            }
            
            // Check for specific help.
            switch (commandName.ToLower())
            {
                case CMD_CONSTANT.PING_COMMAND_NAME:
                    break;
                
                case CMD_CONSTANT.HELP_COMMAND_NAME:
                    break;

                case CMD_CONSTANT.EMBED_COMMAND_NAME:
                    break;
            }
        } while (!string.IsNullOrEmpty(commandName));
    }

    public static async Task<string?> Main(DiscordUser user, DiscordMessage msgHandler)
    {
        // Create initial embed.
        var embed = new DiscordEmbedBuilder() {
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            Description = $"Here's the summary list of commands:\n"
                + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.PING_COMMAND_NAME}`: {CMD_CONSTANT.PING_COMMAND_DESCRIPTION}\n"
                + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.HELP_COMMAND_NAME}`: {CMD_CONSTANT.HELP_COMMAND_DESCRIPTION}\n",
            Title = "Need Help?",
        };
        var messageBuilder = new DiscordMessageBuilder().AddEmbed(embed);

        // Edit help message interaction.
        await msgHandler.ModifyAsync(messageBuilder);

        return null;
    }

    #endregion
}
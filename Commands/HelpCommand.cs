using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public static class HelpCommand
{
    public static readonly DiscordEmbedBuilder MAIN_HELP_CONTEXT_EMBED = new DiscordEmbedBuilder() {
        Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
        Description = $"Here's the summary list of commands:\n"
            + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.PING_COMMAND_NAME}`: {CMD_CONSTANT.PING_COMMAND_DESCRIPTION}\n"
            + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.HELP_COMMAND_NAME}`: {CMD_CONSTANT.HELP_COMMAND_DESCRIPTION}\n"
            + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.SET_BUTTON_COMMAND_NAME}`: {CMD_CONSTANT.SET_BUTTON_COMMAND_DESCRIPTION}`\n"
            + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.REMOVE_BUTTON_COMMAND_NAME}`: {CMD_CONSTANT.REMOVE_BUTTON_COMMAND_DESCRIPTION}`\n"
            + $"`{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.ADD_FORM_COMMAND_NAME}`: {CMD_CONSTANT.ADD_FORM_COMMAND_DESCRIPTION}`\n",
        Title = "HELP (Main Page)",
    };

    public static async Task Help(InteractionContext ctx, string? commandName = null)
    {
        // Check any command for help.
        if (string.IsNullOrEmpty(commandName))
        {
            await MainHelp(ctx);
            return;
        }
        
        // Check for specific help.
        switch (commandName.ToLower())
        {
            case CMD_CONSTANT.PING_COMMAND_NAME:
                break;
            
            case CMD_CONSTANT.HELP_COMMAND_NAME:
                break;

            case CMD_CONSTANT.SET_BUTTON_COMMAND_NAME:
                break;

            case CMD_CONSTANT.REMOVE_BUTTON_COMMAND_NAME:
                break;

            case CMD_CONSTANT.ADD_FORM_COMMAND_NAME:
                break;
        }
    }
    
    public static async Task MainHelp(InteractionContext ctx)
    {
        var interactivity = ctx.Client.GetInteractivity();

        // Create initial embed.
        var mainEmbed = MAIN_HELP_CONTEXT_EMBED;
        mainEmbed.Author = new DiscordEmbedBuilder.EmbedAuthor {
            IconUrl = ctx.Client.CurrentUser.AvatarUrl,
            Name = ctx.Client.CurrentUser.Username,
        };
        var messageBuilder = new DiscordWebhookBuilder().AddEmbed(mainEmbed);

        // Begin response.
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await Task.Delay(100);

        // Edit help message interaction.
        await ctx.Interaction.EditOriginalResponseAsync(messageBuilder);
    }

    public static async Task Help(CommandContext ctx, string? commandName = null)
    {
        var interactivity = ctx.Client.GetInteractivity();
        
        // Create initial embed.
        var mainEmbed = MAIN_HELP_CONTEXT_EMBED;
        mainEmbed.Author = new DiscordEmbedBuilder.EmbedAuthor {
            IconUrl = ctx.Client.CurrentUser.AvatarUrl,
            Name = ctx.Client.CurrentUser.Username,
        };
        var messageBuilder = new DiscordMessageBuilder().AddEmbed(mainEmbed);
        
        await ctx.Message.RespondAsync(messageBuilder);
    }
}
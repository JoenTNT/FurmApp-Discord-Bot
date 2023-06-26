using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public static class PrefixCommand
{
    public static async Task GetPrefix(InteractionContext ctx)
    {
        var content = $"The Prefix of this Server is \"{CONSTANT.DEFAULT_PREFIX}\"";
        var response = new DiscordInteractionResponseBuilder().WithContent(content);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    public static async Task GetPrefix(CommandContext ctx)
    {
        var content = $"The Prefix of this Server is \"{CONSTANT.DEFAULT_PREFIX}\"";

        await ctx.RespondAsync(content);
    }
}
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

// TODO: Follow the template.
public static class PingCommand
{
    public static async Task Ping(InteractionContext ctx)
    {
        var reply = $"Pong! {ctx.Client.Ping}ms";

        var embed = new DiscordEmbedBuilder()
        {
            Description = reply,
            Color = new DiscordColor(0xFFFFFF),
        };

        var response = new DiscordInteractionResponseBuilder().AddEmbed(embed);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    public static async Task Ping(CommandContext ctx)
    {
        var reply = $"Pong! {ctx.Client.Ping}ms";

        var embed = new DiscordEmbedBuilder()
        {
            Description = reply,
            Color = new DiscordColor(0xFFFFFF),
        };

        await ctx.RespondAsync(embed);
    }
}
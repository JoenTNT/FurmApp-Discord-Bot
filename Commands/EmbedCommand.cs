using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public static class EmbedCommand
{
    public static async Task Embed(InteractionContext ctx, string? authorIconUrl, string? authorName, string? authorHyperLink,
        string? embedColor, string? embedDesc, string? footerIconUrl, string? footerText, string? imageUrl, string? thumbnailUrl,
        string? title, string? titleUrl)
    {
        DiscordEmbed embed = new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor()
            { 
                IconUrl = authorIconUrl,
                Name = authorName,
                Url = authorHyperLink,
            },
            Color = new DiscordColor(embedColor),
            Description = embedDesc,
            Footer = new DiscordEmbedBuilder.EmbedFooter()
            { 
                IconUrl = footerIconUrl,
                Text = footerText,
            },
            ImageUrl = imageUrl,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = thumbnailUrl,
            },
            Timestamp = DateTime.UtcNow,
            Title = title,
            Url = titleUrl,
        };

        var response = new DiscordInteractionResponseBuilder().AddEmbed(embed);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
}
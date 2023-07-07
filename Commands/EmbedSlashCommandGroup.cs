using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.EMBED_COMMAND_NAME, CMD_CONSTANT.EMBED_COMMAND_DESCRIPTION)]
public class EmbedSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.CREATE_COMMAND_NAME, CMD_CONSTANT.EMBED_CREATE_DESCRIPTION)]
    public async Task Create(InteractionContext ctx,
        [Option(CMD_CONSTANT.EMBED_AUTHOR_ICON_URL_PARAMETER, "Set icon of author part in embed.")]
        string? authorIconUrl = null,
        [Option(CMD_CONSTANT.EMBED_AUTHOR_NAME_PARAMETER, "Set author name in embed.")]
        string? authorName = null,
        [Option(CMD_CONSTANT.EMBED_COLOR_PARAMETER, "Set embed color, default is 'white' or 'FFFFFF'.")]
        string? embedColor = CMD_CONSTANT.EMBED_COLOR_PARAMETER,
        [Option(CMD_CONSTANT.EMBED_DESCRIPTION_PARAMETER, "Embed main description under the title.")]
        string? embedDesc = null,
        [Option(CMD_CONSTANT.EMBED_FOOTER_ICON_URL_PARAMETER, "Set icon of footer part in embed.")]
        string? footerIconUrl = null,
        [Option(CMD_CONSTANT.EMBED_FOOTER_TEXT_PARAMETER, "Set footer text in embed.")]
        string? footerText = null,
        [Option(CMD_CONSTANT.EMBED_IMAGE_URL_PARAMETER, "Set embed main image attachment.")]
        string? imageUrl = null,
        [Option(CMD_CONSTANT.EMBED_THUMBNAIL_URL_PARAMETER, "Set thumbnail in embed.")]
        string? thumbnailUrl = null,
        [Option(CMD_CONSTANT.EMBED_TITLE_PARAMETER, "Set title of embed.")]
        string? title = null)
    {
        // Delete interaction by default.
        await ctx.DeferAsync();
        await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Please wait for a moment..."));

        // Create embed.
        await EmbedCommandsModule.Create(ctx.Channel, authorIconUrl, authorName, embedColor,
            embedDesc, footerIconUrl, footerText, imageUrl, thumbnailUrl, title);
        //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Embed created!");

        await ctx.DeleteResponseAsync();
    }
    
    [SlashCommand(CMD_CONSTANT.EDIT_COMMAND_NAME, CMD_CONSTANT.EMBED_EDIT_DESCRIPTION)]
    public async Task Edit(InteractionContext ctx,
        [Option("MessageID", "Target message ID")]
        string messageID,
        [Option("Element", "Target element in that embed.")]
        string element,
        [Option("Value", "Set value of the element, value may be vary and may not be always work.")]
        string value)
    {
        await ctx.DeleteResponseAsync();
        try
        {
            DiscordMessage msg = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
            await EmbedCommandsModule.Edit(msg, element, value);
        }
        catch (Exception)
        {
            // TODO: Handle exception.
            return;
        }
    }
}
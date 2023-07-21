using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.EMBED_COMMAND_NAME, CMD_CONSTANT.EMBED_COMMAND_DESCRIPTION)]
public class EmbedSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.CREATE_COMMAND_NAME, CMD_CONSTANT.EMBED_CREATE_COMMAND_DESCRIPTION)]
    public async Task Create(InteractionContext ctx,
        [Option(CMD_CONSTANT.EMBED_AUTHOR_ICON_URL_PARAMETER, "Set icon of author part in embed.")]
        string? authorIconUrl = null,
        [Option(CMD_CONSTANT.EMBED_AUTHOR_NAME_PARAMETER, "Set author name in embed.")]
        string? authorName = null,
        [Option(CMD_CONSTANT.EMBED_COLOR_PARAMETER, "Set embed color, default is 'white' or 'FFFFFF'.")]
        string? embedColor = null,
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
        // Replace interaction with normal message handler instead of slash command.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        await ctx.DeleteResponseAsync();

        // Create embed.
        await EmbedCommandsModule.Create(ctx.Channel, authorIconUrl, authorName, embedColor,
            embedDesc, footerIconUrl, footerText, imageUrl, thumbnailUrl, title);

        // Delete message handler.
        await msgHandler.DeleteAsync();
    }
    
    // TODO: Create an edit command for embed.
    [SlashCommand(CMD_CONSTANT.EDIT_COMMAND_NAME, CMD_CONSTANT.EMBED_EDIT_COMMAND_DESCRIPTION)]
    public async Task Edit(InteractionContext ctx,
        [Option(CMD_CONSTANT.MESSAGE_ID_PARAMETER, CMD_CONSTANT.MESSAGE_ID_PARAMETER_DESCRIPTION)]
        string messageID,
        [Option(CMD_CONSTANT.ELEMENT_PARATEMER, CMD_CONSTANT.EMBED_ELEMENT_PARAMETER_DESCRIPTION)]
        string element,
        [Option(CMD_CONSTANT.VALUE_PARAMETER, CMD_CONSTANT.EMBED_VALUE_PARAMETER_DESCRIPTION)]
        string value)
    {
        // await ctx.DeleteResponseAsync();
        // try
        // {
        //     DiscordMessage msg = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
        //     await EmbedCommandsModule.Edit(msg, element, value);
        // }
        // catch (Exception)
        // {
        //     // TODO: Handle exception.
        //     return;
        // }
    }
}
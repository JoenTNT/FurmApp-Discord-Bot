using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FurmAppDBot.Clients;

namespace FurmAppDBot.Commands;

public class EmbedCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.EMBED_COMMAND_NAME)]
    public async Task Embed(CommandContext ctx, string subCommand, params string[] inputs)
    {
        (string?[], bool) ps;
        switch (subCommand.ToLower())
        {
            case CMD_CONSTANT.CREATE_COMMAND_NAME:
                //FurmAppClient.Instance.Logger.LogInformation($"Sub Command: {subCommand};");
                //FurmAppClient.Instance.Logger.LogInformation($"Parameter Input Count: {inputs.Count()};");
                //foreach (var i in inputs) FurmAppClient.Instance.Logger.LogInformation($"{i}");
                try
                {
                    CreateDefinition(inputs, out ps.Item1, out ps.Item2);
                }
                catch (Exception ex)
                {
                    FurmAppClient.Instance.Logger.LogError(ex, $"Command Called by: {ctx.User}");
                    await ctx.Channel.SendMessageAsync("```Bad input argument, abort process.```");
                    return;
                }
                //await ctx.Message.DeleteAsync();
                //foreach (var i in ps.Item1) FurmAppClient.Instance.Logger.LogInformation($"{i}");
                
                // Check if there's no defined element.
                if (!ps.Item2)
                {
                    // Abort process
                    await ctx.Channel.SendMessageAsync("```Bad input argument, abort process.```");
                    return;
                }

                //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Start creating embed...");
                await Create(ctx.Channel, ps.Item1[0],ps.Item1[1], ps.Item1[2],
                    ps.Item1[3], ps.Item1[4], ps.Item1[5], ps.Item1[6], ps.Item1[7], ps.Item1[8]);
                break;

            case CMD_CONSTANT.EDIT_COMMAND_NAME:
                break;
        }
    }

    /// <summary>
    /// Convert command parameters to be sorted before creating embed by correct parameter.
    /// If there are more than one element-value pair, then it will take the first parameter input.
    /// </summary>
    /// <param name="elementValue">Unsorted parameter definitions</param>
    /// <returns>Sorted definitions, second argument means that no elements defined.</returns>
    /// <exception cref="Exception">
    /// Template input must be seperate by equal and value must not be empty, example: '<Element>=<Value>'.
    /// </exception>
    private void CreateDefinition(string[] elementValue, out string?[] result, out bool foundAny)
    {
        result = new string?[9];
        foundAny = false;

        // Define author's icon URL.
        string? p0 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_AUTHOR_ICON_URL_PARAMETER));
        if (p0 != null)
        {
            result[0] = GetValueAtFirstEqual(p0);
            if (!string.IsNullOrEmpty(result[0])) foundAny = true;
        }

        // Define author's name.
        string? p1 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_AUTHOR_NAME_PARAMETER));
        if (p1 != null)
        {
            result[1] = GetValueAtFirstEqual(p1);
            if (!string.IsNullOrEmpty(result[1])) foundAny = true;
        }

        // Define embed's color.
        string? p2 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_COLOR_PARAMETER));
        if (p2 != null)
        {
            result[2] = GetValueAtFirstEqual(p2);
            if (!string.IsNullOrEmpty(result[2])) foundAny = true;
        }

        // Define embed's description.
        string? p3 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_DESCRIPTION_PARAMETER));
        if (p3 != null)
        {
            result[3] = GetValueAtFirstEqual(p3);
            if (!string.IsNullOrEmpty(result[3])) foundAny = true;
        }

        // Define footer's icon URL.
        string? p4 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_FOOTER_ICON_URL_PARAMETER));
        if (p4 != null)
        {
            result[4] = GetValueAtFirstEqual(p4);
            if (!string.IsNullOrEmpty(result[4])) foundAny = true;
        }

        // Define footer's text.
        string? p5 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_FOOTER_TEXT_PARAMETER));
        if (p5 != null)
        {
            result[5] = GetValueAtFirstEqual(p5);
            if (!string.IsNullOrEmpty(result[5])) foundAny = true;
        }

        // Define embed's image attachment URL
        string? p6 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_IMAGE_URL_PARAMETER));
        if (p6 != null)
        {
            result[6] = GetValueAtFirstEqual(p6);
            if (!string.IsNullOrEmpty(result[6])) foundAny = true;
        }

        // Define embed's image thumbnail URL.
        string? p7 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_THUMBNAIL_URL_PARAMETER));
        if (p7 != null)
        {
            result[7] = GetValueAtFirstEqual(p7);
            if (!string.IsNullOrEmpty(result[7])) foundAny = true;
        }

        // Define embed's title text.
        string? p8 = elementValue.FirstOrDefault((i) => i.ToLower().StartsWith(CMD_CONSTANT.EMBED_TITLE_PARAMETER));
        if (p8 != null)
        {
            result[8] = GetValueAtFirstEqual(p8);
            if (!string.IsNullOrEmpty(result[8])) foundAny = true;
        }
    }

    #endregion

    #region Statics

    /// <summary>
    /// Creates an embed and send it in target channel.
    /// </summary>
    /// <param name="channel">Target channel</param>
    /// <param name="authorIconUrl">Author's icon URL, must be image link</param>
    /// <param name="authorName">Author's name</param>
    /// <param name="embedColor">Embed's color, must be hex color value, example: '050505' (almost complete Black)</param>
    /// <param name="embedDesc">Embed's main description</param>
    /// <param name="footerIconUrl">Footer's icon URL, must be image link</param>
    /// <param name="footerText">Footer's text</param>
    /// <param name="imageUrl">Embed's image attachment URL, must be image link</param>
    /// <param name="thumbnailUrl">Embed's image thumbnail URL, must be image link</param>
    /// <param name="title">Embed's title text</param>
    public static async Task<DiscordMessage?> Create(DiscordChannel channel, string? authorIconUrl, string? authorName, string? embedColor,
        string? embedDesc, string? footerIconUrl, string? footerText, string? imageUrl, string? thumbnailUrl, string? title)
    {
        DiscordEmbed embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor()
            { 
                IconUrl = authorIconUrl,
                Name = authorName,
            },
            Color = new DiscordColor(string.IsNullOrEmpty(embedColor) ? CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT : embedColor),
            Description = string.IsNullOrEmpty(embedDesc) ? embedDesc : embedDesc.Replace("\\n", "\n"),
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
        };

        try
        {
            //FurmAppClient.Instance.Logger.LogInformation("[DEBUG] Sending embed...");
            return await channel.SendMessageAsync(embed);
        }
        catch (Exception ex)
        {
            FurmAppClient.Instance.Logger.LogInformation(ex, string.Empty);
            return default(DiscordMessage);
        }
    }

    /// <summary>
    /// Edit embed message.
    /// This method assume if the message has only one embed.
    /// </summary>
    /// <param name="msg">Target discord message</param>
    /// <param name="elementName">One of the embed's element name</param>
    /// <param name="value">Value to be set, may be vary one element and another</param>
    public static async Task<DiscordMessage> Edit(DiscordMessage msg, string elementName, string value)
    {
        await Task.CompletedTask;
        return msg;
    }

    private static string? GetValueAtFirstEqual(string input)
    {
        int startAt = input.IndexOf('=');
        return input.Substring(startAt + 1);
    }

    #endregion
}
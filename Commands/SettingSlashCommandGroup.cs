using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.SETTING_COMMAND_NAME, CMD_CONSTANT.SETTING_COMMAND_DESCRIPTION)]
public class SettingSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.CATEGORY_COMMAND_NAME, CMD_CONSTANT.SETTING_CATEGORY_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Category(InteractionContext ctx,
        [Option(CMD_CONSTANT.CHANNEL_CATEGORY_ID_PARAMETER, CMD_CONSTANT.CHANNEL_CATEGORY_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(ChannelCategoryAutocompleteProvider))]
        string? channelCategoryID = null)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();
            
            try
            {
                // Check if user is using command in server as member.
                if (ctx.Member == null)
                {
                    // Notify message handler.
                    await msgHandler.ModifyAsync("This command only can be used in server, abort the process.");
                    return;
                }

                // Start setting category.
                await SettingCommandsModule.SetCategory(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id),
                    msgHandler, ctx.Member, string.IsNullOrEmpty(channelCategoryID) ? null : ulong.Parse((string)channelCategoryID));
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Bad argument detected, some insert parameters are incorrect, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.INFO_COMMAND_NAME, CMD_CONSTANT.SETTING_INFO_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Info(InteractionContext ctx)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();

            try
            {
                // Send information to user in server.
                await SettingCommandsModule.Info(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id),
                    new DiscordEmbedBuilder.EmbedAuthor {
                        Name = ctx.Client.CurrentUser.Username,
                        IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                    }, msgHandler, ctx.Member);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
}
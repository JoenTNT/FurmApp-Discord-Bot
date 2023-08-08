using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.CONTAINER_COMMAND_NAME, CMD_CONSTANT.CHANNEL_CONTAINER_COMMAND_DESCRIPTION)]
public class ContainerSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.SET_COMMAND_NAME, CMD_CONSTANT.CC_SET_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Set(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID,
        [Option(CMD_CONSTANT.CHANNEL_ID_PARAMETER, CMD_CONSTANT.CHANNEL_ID_PARAMETER_DESCRIPTION)]
        string channelID)
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
                // Check if form does exists.
                FormData data = await FormData.GetData(ctx.Guild.Id, formID);
                
                // Try get channel if exists.
                DiscordChannel channel;
                try
                {
                    // Get all channels.
                    var channels = ctx.Guild.Channels;

                    // Check specific channel exists.
                    ulong cid = ulong.Parse(channelID);
                    if (!channels.ContainsKey(cid))
                    {
                        await msgHandler.ModifyAsync("Channel with ID not found, abort the process.");
                        return;
                    }

                    // Get specific channel.
                    channel = ctx.Guild.Channels[cid];
                }
                catch (FormatException) // Wrong input format.
                {
                    // Notify by message handler.
                    await msgHandler.ModifyAsync("Channel ID must be numbers only, abort the process.");
                    return;
                }
                
                // Start setting process.
                await ContainerCommandsModule.Set(msgHandler, data, channel);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
}

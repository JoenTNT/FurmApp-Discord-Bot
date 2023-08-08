using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

// TODO: Form container commands.
public class ContainerCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.CONTAINER_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Container(CommandContext ctx, string commandName, string formID, string? channelID = null)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Check if form does exists.
                FormData data = await FormData.GetData(ctx.Guild.Id, formID);

                // Check which command run.
                switch (commandName.ToLower())
                {
                    case CMD_CONSTANT.SET_COMMAND_NAME when !string.IsNullOrEmpty(channelID):
                        // Check if channel is inputed.
                        if (string.IsNullOrEmpty(channelID))
                        {
                            await msgHandler.ModifyAsync("Please provide channel ID, abort the process.");
                            return;
                        }

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
                        await Set(msgHandler, data, channel);
                        break;
                    
                    case CMD_CONSTANT.GET_COMMAND_NAME:
                        // TODO: Get Channel container information.
                        break;
                }
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormNotFoundException) // When getting form is not found.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Form with ID `{formID}` does not exists, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    #endregion

    #region Statics

    public static async Task Set(DiscordMessage msgHandler, FormData data, DiscordChannel channel)
    {
        // Setting form channel container.
        data.SetChannelAsContainer(channel.Id);

        // Save form update.
        await data.SaveData();

        // Notify changes.
        await msgHandler.ModifyAsync($"Successfully Set! Each user submission of form `{data.FormID}` will be send to "
            + $"<#{channel.Id}> from now on.");
    }

    #endregion
}
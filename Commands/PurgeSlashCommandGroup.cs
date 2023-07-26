
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public class PurgeSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.PURGE_COMMAND_NAME, CMD_CONSTANT.PURGE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.Administrator)]
    public async Task Purge(InteractionContext ctx,
        [Option(CMD_CONSTANT.AMOUNT_PARAMETER, CMD_CONSTANT.PURGE_AMOUNT_PARAMETER_DESCRIPTION)]
        string purgeAmount)
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
                // Start purging process.
                await PurgeCommandsModule.Purge(msgHandler, ctx.Channel, int.Parse(purgeAmount));
            }
            catch (NotFoundException) // When purging is interrupted.
            {
                // Notify message handler.
                await msgHandler.ModifyAsync("Purging has been interrupted, abort the process.");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Purge amount must be number, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
}
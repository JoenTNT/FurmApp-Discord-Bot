using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public class PingSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.PING_COMMAND_NAME, CMD_CONSTANT.PING_COMMAND_DESCRIPTION)]
    public async Task Ping(InteractionContext ctx)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Initial respond with message handler.
        DiscordMessage msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();

            // Run ping command.
            await PingCommandsModule.Ping(msgHandler, ctx.Client);
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
}
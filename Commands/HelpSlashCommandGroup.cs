using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands;

public class HelpSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.HELP_COMMAND_NAME, CMD_CONSTANT.HELP_COMMAND_DESCRIPTION)]
    public async Task Help(InteractionContext ctx,
        [Option(CMD_CONSTANT.COMMAND_NAME_PARAMETER, CMD_CONSTANT.HELP_COMMAND_NAME_PARAMETER_DESCRIPTION)]
        [ChoiceProvider(typeof(CommandNameChoiceProvider))]
        string? commandName = null)
    {
        // Deferring interaction.
        await ctx.DeferAsync();

        // Intialize message handler.
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

        try
        {
            // Delete slash command interaction.
            await ctx.DeleteResponseAsync();

            // Set initial command name to main if user didn't input any.
            if (string.IsNullOrEmpty(commandName)) commandName = HelpCommandsModule.MAIN_HELP_PAGE_KEY;

            // Start by choosing command.
            await HelpCommandsModule.ChooseCommand(ctx.Client, ctx.User, msgHandler, commandName);
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }
}
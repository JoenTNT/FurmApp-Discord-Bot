using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands;

public class HelpSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.HELP_COMMAND_NAME, CMD_CONSTANT.HELP_COMMAND_DESCRIPTION)]
    public async Task Help(InteractionContext ctx,
        [Option("CommandName", "Name the command if you need a specific help.", autocomplete: false)]
        [ChoiceProvider(typeof(CommandNameChoiceProvider))]
        string? commandName = null)
    {
        // Handle slash command respond.
        await ctx.DeferAsync();

        // Intialize message handler.
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        await ctx.DeleteResponseAsync();

        try
        {
            // Set initial command name to main if user didn't input any.
            if (string.IsNullOrEmpty(commandName)) commandName = HelpCommandsModule.MAIN_HELP_PAGE_KEY;

            // Start by choosing command.
            await HelpCommandsModule.ChooseCommand(ctx.Client, ctx.User, msgHandler, commandName);
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
    }
}
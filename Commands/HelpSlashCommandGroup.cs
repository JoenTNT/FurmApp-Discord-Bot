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

        await HelpCommandsModule.ChooseCommand(ctx.User, msgHandler, commandName);
    }
}
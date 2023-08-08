using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

// TODO: Prefix Command.
public class PrefixCommandsModule : BaseCommandModule
{
    // TODO: Prefix Command.
    // [Command(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME)]
    // public async Task GetPrefix(CommandContext ctx)
    //     => await PrefixCommand.GetPrefix(ctx);
    
    [Command(CMD_CONSTANT.PREFIX_COMMAND_NAME)]
    public async Task Prefix(CommandContext ctx, string commandName, params string[] inputs)
    {
        // TODO: Change prefix for specific Guild.
        await Task.CompletedTask;
    }
    
    // public static async Task GetPrefix(InteractionContext ctx)
    // {
    //     var content = $"The Prefix of this Server is \"{CONSTANT.DEFAULT_PREFIX}\"";
    //     var response = new DiscordInteractionResponseBuilder().WithContent(content);

    //     await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    // }

    // public static async Task GetPrefix(CommandContext ctx)
    // {
    //     var content = $"The Prefix of this Server is \"{CONSTANT.DEFAULT_PREFIX}\"";

    //     await ctx.RespondAsync(content);
    // }
}
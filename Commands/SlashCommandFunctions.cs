using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands;

public class SlashCommandFunctions : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.PING_COMMAND_NAME, CMD_CONSTANT.PING_COMMAND_DESCRIPTION)]
    public async Task Ping(InteractionContext ctx)
        => await PingCommand.Ping(ctx);

    [SlashCommand(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME, CMD_CONSTANT.GET_PREFIX_COMMAND_DESCRIPTION)]
    public async Task GetPrefix(InteractionContext ctx)
        => await PrefixCommand.GetPrefix(ctx);

    [SlashCommand(CMD_CONSTANT.PURGE_COMMAND_NAME, CMD_CONSTANT.PURGE_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Purge(InteractionContext ctx,
        [Option(CMD_CONSTANT.AMOUNT_PARAMETER, CMD_CONSTANT.PURGE_AMOUNT_PARAMETER_DESCRIPTION, true)]
        string purgeAmount)
        => await PurgeCommand.Purge(ctx, purgeAmount);

    // // TEMPORARY: For testing modal form feature in Discord.
    // [SlashCommand("samplemodal", "For testing a sample modal")]
    // public async Task SampleModal(InteractionContext ctx)
    // {
    //     try
    //     {
    //         InteractivityExtension interactivity = ctx.Client.GetInteractivity();

    //         // var actionRows = new DiscordActionRowComponent(new DiscordComponent[] {
    //         //     new TextInputComponent("This is Part A", "input_1", max_length: 64)
    //         //     {
    //         //         Placeholder = "Write anything you like...",
    //         //         Style = TextInputStyle.Short,
    //         //         Required = true,
    //         //     },
    //         // });

    //         var modalID = "modal_x";
    //         DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
    //             .WithTitle("This is a Sample Modal")
    //             .WithContent("This is a sample modal, like a form, for polling and survey stuff.")
    //             .WithCustomId(modalID)
    //             .AddComponents(
    //                 new TextInputComponent("This is Part A", "input_1", max_length: 64)
    //                 {
    //                     Placeholder = "Write anything you like...",
    //                     Style = TextInputStyle.Short,
    //                     Required = true,
    //                 }
    //             )
    //             .AddComponents(
    //                 new TextInputComponent("This is Part B", "input_2", max_length: 256)
    //                 {
    //                     Placeholder = "Write another thing you like...",
    //                     Style = TextInputStyle.Paragraph,
    //                     Required = false,
    //                 }
    //             );
            
    //         await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
    //         var submittedModal = await interactivity.WaitForModalAsync(modalID, ctx.User);

    //         await submittedModal.Result.Interaction.CreateResponseAsync(
    //             InteractionResponseType.ChannelMessageWithSource,
    //             new DiscordInteractionResponseBuilder().WithContent("Form has been submitted. " 
    //                 + $"(Result: {submittedModal.Result.Values["input_2"]})")
    //         );
    //     }
    //     catch (Exception ex)
    //     {
    //         FurmAppClient.Instance.Logger.LogError(ex, string.Empty);
    //     }
    // }
}
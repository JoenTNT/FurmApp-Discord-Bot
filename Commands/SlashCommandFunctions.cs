using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands.Providers;

namespace FurmAppDBot.Commands
{
    public class SlashCommandFunctions : ApplicationCommandModule
    {
        [SlashCommand(CMD_CONSTANT.PING_COMMAND_NAME, CMD_CONSTANT.PING_COMMAND_DESCRIPTION)]
        public async Task Ping(InteractionContext ctx) => await PingCommand.Ping(ctx);

        [SlashCommand(CMD_CONSTANT.GET_PREFIX_COMMAND_NAME, CMD_CONSTANT.GET_PREFIX_COMMAND_DESCRIPTION)]
        public async Task GetPrefix(InteractionContext ctx) => await PrefixCommand.GetPrefix(ctx);

        [SlashCommand(CMD_CONSTANT.EMBED_COMMAND_NAME, CMD_CONSTANT.EMBED_COMMAND_DESCRIPTION)]
        public async Task Embed(InteractionContext ctx,
            [Option("AuthorIconURL", "Set icon of author part in embed.")]
            string? authorIconUrl = null,
            [Option("AuthorName", "Set author name in embed.")]
            string? authorName = null,
            [Option("AuthorHyperLink", "Set author hyper link.")]
            string? authorHyperLink = null,
            [Option("EmbedColor", "Set embed color, default is 'white' or 'FFFFFF'.")]
            string? embedColor = "FFFFFF",
            [Option("EmbedDescription", "Embed main description under the title.")]
            string? embedDesc = null,
            [Option("FooterIconURL", "Set icon of footer part in embed.")]
            string? footerIconUrl = null,
            [Option("FooterText", "Set footer text in embed.")]
            string? footerText = null,
            [Option("ImageAttachmentURL", "Set embed main image attachment.")]
            string? imageUrl = null,
            [Option("ThumbnailAttachmentURL", "Set thumbnail in embed.")]
            string? thumbnailUrl = null,
            [Option("Title", "Set title of embed.")]
            string? title = null,
            [Option("TitleURL", "Set title hyper link of embed.")]
            string? titleUrl = null) => await EmbedCommand.Embed(ctx, authorIconUrl, authorName, authorHyperLink, embedColor,
                embedDesc, footerIconUrl, footerText, imageUrl, thumbnailUrl, title, titleUrl);
        
        [SlashCommand(CMD_CONSTANT.PURGE_COMMAND_NAME, CMD_CONSTANT.PURGE_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task Purge(InteractionContext ctx,
            [Option("Amount", "How many messages will be deleted before calling this command. For example: 10", true)]
            string purgeAmount) => await PurgeCommand.Purge(ctx, purgeAmount);

        [SlashCommand(CMD_CONSTANT.SET_BUTTON_COMMAND_NAME, CMD_CONSTANT.SET_BUTTON_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task SetButton(InteractionContext ctx,
            [Option("MessageID", "Target message ID, For Example: 1234567890123456789", autocomplete: true)]
            [ChoiceProvider(typeof(MessageIDChoiceProvider))]
            string messageID) => await ButtonCommand.SetButton(ctx, messageID);

        [SlashCommand(CMD_CONSTANT.GET_BUTTON_COMMAND_NAME, CMD_CONSTANT.GET_BUTTON_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task GetButton(InteractionContext ctx,
            [Option("MessageID", "Target message ID, For Example: 1234567890123456789", autocomplete: true)]
            [ChoiceProvider(typeof(MessageIDChoiceProvider))]
            string messageID) => await ButtonCommand.GetButton(ctx, messageID);

        [SlashCommand(CMD_CONSTANT.REMOVE_BUTTON_COMMAND_NAME, CMD_CONSTANT.REMOVE_BUTTON_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task RemoveButton(InteractionContext ctx,
            [Option("MessageID", "Target message ID, For Example: 1234567890123456789", autocomplete: true)]
            [ChoiceProvider(typeof(MessageIDChoiceProvider))]
            string messageID,
            [Option("ButtonID", "Target button ID", autocomplete: false)]
            string? buttonID = null) => await ButtonCommand.RemoveButton(ctx, messageID, buttonID);

        [SlashCommand(CMD_CONSTANT.HELP_COMMAND_NAME, CMD_CONSTANT.HELP_COMMAND_DESCRIPTION)]
        public async Task Help(InteractionContext ctx) => await HelpCommand.Help(ctx);

        [SlashCommand(CMD_CONSTANT.ADD_FORM_COMMAND_NAME, CMD_CONSTANT.ADD_FORM_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task AddForm(InteractionContext ctx,
            [Option("FormID", "New form by ID.", autocomplete: true)]
            string formID) => await FormCommand.AddForm(ctx, formID);

        [SlashCommand(CMD_CONSTANT.GET_ALL_FORM_COMMAND_NAME, CMD_CONSTANT.GET_ALL_FORM_COMMAND_DESCRIPTION)]
        [SlashCommandPermissions(Permissions.ManageGuild)]
        public async Task GetAllForm(InteractionContext ctx) => await FormCommand.GetAllForm(ctx);

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
}
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands.Providers;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

[SlashCommandGroup(CMD_CONSTANT.FORM_COMMAND_NAME, CMD_CONSTANT.FORM_COMMAND_DESCRIPTION)]
public class FormSlashCommandGroup : ApplicationCommandModule
{
    [SlashCommand(CMD_CONSTANT.CREATE_COMMAND_NAME, CMD_CONSTANT.CREATE_FORM_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Create(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION)]
        string formID)
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
                // Getting all informations.
                await FormCommandsModule.Add(msgHandler, formID);
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    [SlashCommand(CMD_CONSTANT.GET_COMMAND_NAME, CMD_CONSTANT.GET_FORM_DETAIL_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Get(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string formID)
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
                // Getting all informations.
                await FormCommandsModule.Get(msgHandler, formID);
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

    [SlashCommand(CMD_CONSTANT.DELETE_COMMAND_NAME, CMD_CONSTANT.DELETE_FORM_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Delete(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION, true)]
        [Autocomplete(typeof(FormIDAutocompleteProvider))]
        string? formID = null)
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
                // Getting all informations.
                await FormCommandsModule.Delete(ctx.User, msgHandler, formID);
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

    [SlashCommand(CMD_CONSTANT.GET_ALL_COMMAND_NAME, CMD_CONSTANT.GET_FORMS_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task GetAll(InteractionContext ctx)
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
                // Getting all informations.
                await FormCommandsModule.GetAll(msgHandler, new DiscordEmbedBuilder.EmbedAuthor {
                    IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                    Name = ctx.Client.CurrentUser.Username,
                });
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    // TEMPORARY: For testing modal form feature in Discord.
    [SlashCommand("samplemodal", "For testing a sample modal")]
    public async Task SampleModal(InteractionContext ctx)
    {
        try
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            var modalID = "sample_modal";
            DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
                .WithTitle("This is a Sample Modal")
                .WithCustomId(modalID)
                .AddComponents(new DiscordComponent[] {
                    new TextInputComponent("This is First Question", "q1", max_length: 64)
                    {
                        Placeholder = "A Placeholder with Optional Short Input.",
                        Style = TextInputStyle.Short,
                        Required = true,
                    }
                })
                .AddComponents(new DiscordComponent[] {
                    new TextInputComponent("This is Second Question?", "q2", max_length: 256)
                    {
                        Placeholder = $"A Placeholder with Required Paragraph Input.",
                        Style = TextInputStyle.Paragraph,
                        Required = false,
                    }
                })
                .AddComponents(new DiscordComponent[] {
                    new TextInputComponent("This is Second Question?", "q2", max_length: 1024, min_length: 100)
                    {
                        Placeholder = $"A Placeholder with Required Paragraph Input.",
                        Style = TextInputStyle.Paragraph,
                        Required = false,
                    }
                });
                
            
            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
            var submittedModal = await interactivity.WaitForModalAsync(modalID, ctx.User,
                TimeSpan.FromSeconds(CONSTANT.FILLING_FORM_IN_SECONDS_DEFAULT_TIMEOUT));

            if (submittedModal.TimedOut)
            {
                await submittedModal.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("TIMEOUT!"));
                return;
            }

            await submittedModal.Result.Interaction.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Form has been submitted. " 
                    + $"(Result: {submittedModal.Result.Values["input_2"]})")
            );
        }
        catch (Exception ex)
        {
            FurmAppClient.Instance.Logger.LogError(ex, string.Empty);
        }
    }
}
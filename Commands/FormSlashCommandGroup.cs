using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
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
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.Add(msgHandler, formID);
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }

    [SlashCommand(CMD_CONSTANT.GET_COMMAND_NAME, CMD_CONSTANT.GET_FORM_DETAIL_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Get(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION)]
        string formID)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.Get(msgHandler, formID);
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }

    [SlashCommand(CMD_CONSTANT.DELETE_COMMAND_NAME, CMD_CONSTANT.DELETE_FORM_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task Delete(InteractionContext ctx,
        [Option(CMD_CONSTANT.FORM_ID_PARAMETER, CMD_CONSTANT.FORM_ID_PARAMETER_DESCRIPTION)]
        string? formID = null)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.Delete(ctx.User, msgHandler, formID);
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }

    [SlashCommand(CMD_CONSTANT.GET_ALL_COMMAND_NAME, CMD_CONSTANT.GET_FORMS_COMMAND_DESCRIPTION)]
    [SlashCommandPermissions(Permissions.ManageGuild)]
    public async Task GetAll(InteractionContext ctx)
    {
        // Initial respond with message handler.
        await ctx.DeferAsync();
        var msgHandler = await ctx.Channel.SendMessageAsync("Please wait for a moment...");
        
        // Proceed the slash command process.
        var rmHandler = await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Proceed!"));

        try
        {
            // Getting all informations.
            await FormCommandsModule.GetAll(msgHandler, new DiscordEmbedBuilder.EmbedAuthor {
                IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                Name = ctx.Client.CurrentUser.Username,
            });
            await rmHandler.DeleteAsync();
        }
        catch (NotFoundException) { /* Ignore the exception if user already deleted the message handler */ }
        catch (DBClientTimeoutException)
        {
            // Database connection has timed out, abort the process.
            await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            await rmHandler.DeleteAsync();
            return;
        }
    }

    // TEMPORARY: For testing modal form feature in Discord.
    [SlashCommand("samplemodal", "For testing a sample modal")]
    public async Task SampleModal(InteractionContext ctx)
    {
        try
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            // var actionRows = new DiscordActionRowComponent(new DiscordComponent[] {
            //     new TextInputComponent("This is Part A", "input_1", max_length: 64)
            //     {
            //         Placeholder = "Write anything you like...",
            //         Style = TextInputStyle.Short,
            //         Required = true,
            //     },
            // });

            var modalID = "sample_modal";
            DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
                .WithTitle("This is a Sample Modal")
                .WithCustomId(modalID)
                .AddComponents(
                    new TextInputComponent("This is First Question", "q1", max_length: 64)
                    {
                        Placeholder = "A Placeholder with Optional Short Input.",
                        Style = TextInputStyle.Short,
                        Required = true,
                    }
                )
                .AddComponents(
                    new TextInputComponent("This is Second Question?", "q2", max_length: 256)
                    {
                        Placeholder = $"A Placeholder with Required Paragraph Input.",
                        Style = TextInputStyle.Paragraph,
                        Required = false,
                    }
                );
            
            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);
            var submittedModal = await interactivity.WaitForModalAsync(modalID, ctx.User, TimeSpan.MaxValue);

            if (submittedModal.TimedOut) return;

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
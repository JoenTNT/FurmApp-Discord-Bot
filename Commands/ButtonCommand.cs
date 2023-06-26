using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;

namespace FurmAppDBot.Commands;

public static class ButtonCommand
{
    private const string SUCCESS_MESSAGE = "Successfully set button on target message! You can now delete this message.";

    public static async Task SetButton(InteractionContext ctx, string messageID)
    {
        async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonTextSkipResponse(
            Task cancellationTask, InteractivityExtension i, DiscordMessage message, DiscordUser user)
        {
            var waiter = i.WaitForButtonAsync(message, user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            if (waiter.Id == result.Id)
            {
                await waiter.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                await Task.Delay(100);
            }

            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        async Task<InteractivityResult<DiscordMessage>> WaitForButtonTextTextResponse(
            Task cancellationTask, InteractivityExtension i, DiscordUser user, DiscordChannel channel)
        {
            var waiter = i.WaitForMessageAsync((message) => {
                //FurmAppClient.Instance.Logger.LogInformation($"The User is the same? {user.Id == message.Author.Id}");
                return user.Id == message.Author.Id && channel.Id == message.Channel.Id;
            });

            Task result = await Task.WhenAny(waiter, cancellationTask);
            
            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForButtonIconReactionResponse(
            Task cancellationTask, InteractivityExtension i, DiscordMessage message, DiscordUser user)
        {
            var waiter = i.WaitForReactionAsync(message, ctx.User, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        try
        {
            // Get target message and declare all variables in method.
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            DiscordMessage? targetMessage = null;
            DiscordInteractionResponseBuilder response;
            DiscordMessageBuilder afterResponseBuilder;
            DiscordWebhookBuilder webhookMessage;
            DiscordMessage messageAfterResponse;
            
            InteractivityResult<ComponentInteractionCreateEventArgs> buttonStylePicked;
            InteractivityResult<MessageReactionAddEventArgs> iconPickResult;
            InteractivityResult<DiscordMessage> buttonIDInput;

            CancellationTokenSource cancelSource;
            Task<InteractivityResult<ComponentInteractionCreateEventArgs>> skipTask;
            Task<InteractivityResult<DiscordMessage>> respondTask;
            Task<InteractivityResult<MessageReactionAddEventArgs>>? reactionTask = null;
            Task cancelTask;
            Task completedTask;
            
            DiscordEmoji? choosenIcon = null;
            ButtonStyle buttonStyle = ButtonStyle.Primary;
            string buttonText = string.Empty;
            string buttonID = string.Empty;

            DiscordMessageBuilder modifyMessageButton;
            DiscordButtonComponent newButtonForm;

            bool iconRequired = false;

            // Convert string to ulong exception handle.
            try
            {
                targetMessage = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
            }
            catch (Exception)
            {
                response = new DiscordInteractionResponseBuilder()
                    .WithContent("Bad argument inserted to message ID, insert only numbers only.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                return;
            }

            // Check if the target message is not found.
            if (targetMessage == null)
            {
                response = new DiscordInteractionResponseBuilder()
                    .WithContent("The Message ID target is either Unavailable or Deleted.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                return;
            }

            // Check if the target message is a user message, this must be prevent due to Discord limitation.
            if (!targetMessage.Author.IsBot)
            {
                response = new DiscordInteractionResponseBuilder()
                    .WithContent("Cannot target the User message.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                return;
            }

            // Start the process
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await Task.Delay(100); // Delay a bit
            
            webhookMessage = new DiscordWebhookBuilder()
                .WithContent("```Choose any Button Style below.```")
                .AddComponents(new DiscordComponent[] {
                    new DiscordButtonComponent(ButtonStyle.Primary, "style1", "Primary"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "style2", "Secondary"),
                    new DiscordButtonComponent(ButtonStyle.Success, "style3", "Success"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "style4", "Danger"),
                });

            // Start with picking Button Style
            messageAfterResponse = await ctx.EditResponseAsync(webhookMessage);
            buttonStylePicked = await interactivity.WaitForButtonAsync(
                messageAfterResponse,
                ctx.User,
                TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            
            // Timeout response if user didn't pick any button style
            if (buttonStylePicked.TimedOut)
            {
                await CallInteractionTimeout(ctx, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                return;
            }

            // Determine which Button used.
            switch (buttonStylePicked.Result.Id)
            {
                case "style1":
                    buttonStyle = ButtonStyle.Primary;
                    break;
                case "style2":
                    buttonStyle = ButtonStyle.Secondary;
                    break;
                case "style3":
                    buttonStyle = ButtonStyle.Success;
                    break;
                case "style4":
                    buttonStyle = ButtonStyle.Danger;
                    break;
            };

            await buttonStylePicked.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            await Task.Delay(100);

            // Edit message to Button Text input content
            afterResponseBuilder = new DiscordMessageBuilder(messageAfterResponse);
            afterResponseBuilder.ClearComponents();
            afterResponseBuilder.Content = "```Send a message to apply Button Text.\n" +
                "Or you can \"Skip\" the Button Text, but the Button Icon is required.```";
            webhookMessage = new DiscordWebhookBuilder(afterResponseBuilder)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            // var followUp = new DiscordFollowupMessageBuilder()
            //     .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            // followUp.Content = "```Send a message to apply Button Text.\n" +
            //     "Or you can \"Skip\" the Button Text, but the Button Icon is required.```";
            messageAfterResponse = await ctx.Interaction.EditOriginalResponseAsync(webhookMessage);

            // Wait for response
            cancelSource = new CancellationTokenSource();
            cancelTask = GetCancellationTask(cancelSource);

            skipTask = WaitForButtonTextSkipResponse(cancelTask, interactivity, messageAfterResponse, ctx.User);
            respondTask = WaitForButtonTextTextResponse(cancelTask, interactivity, ctx.User, ctx.Channel);
            
            completedTask = await Task.WhenAny(skipTask, respondTask);
            cancelSource.Cancel();

            // FurmAppClient.Instance.Logger.LogInformation($"\nSkip Task ID = {skipTask.Id}\n"
            //     + $"Respond Task ID = {respondTask.Id}\n"
            //     + $"Completed Task: {completedTask.Id}\n");

            // Check skipped response.
            if (completedTask == skipTask)
            {
                // Timeout response if not sending any Button Text message.
                if (skipTask.Result.TimedOut)
                {
                    await CallInteractionTimeout(ctx, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                iconRequired = true;
            }

            // Check result response.
            if (completedTask == respondTask)
            {
                iconRequired = false;
                buttonText = respondTask.Result.Result.Content;
                await respondTask.Result.Result.DeleteAsync();
                respondTask.Dispose();
            }
            
            // Edit message to Button Icon input content
            afterResponseBuilder = new DiscordMessageBuilder(messageAfterResponse);
            afterResponseBuilder.ClearComponents();
            afterResponseBuilder.Content = $"```[Icon for Button is {(iconRequired ? "REQUIRED" : "OPTIONAL")}]\n" +
                $"{(iconRequired ? "Please send an Icon with reaction for your button."
                : "Would you like to add an Icon? React with an Icon or \"Skip\".")}```";
            webhookMessage = new DiscordWebhookBuilder(afterResponseBuilder);
            if (!iconRequired)
                webhookMessage.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            messageAfterResponse = await ctx.Interaction.EditOriginalResponseAsync(webhookMessage);

            // Wait for user response
            if (iconRequired) // If the icon is required
            {
                iconPickResult = await interactivity.WaitForReactionAsync(
                    messageAfterResponse, ctx.User, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
                
                // Timeout response if not sending any reaction message.
                if (iconPickResult.TimedOut)
                {
                    await CallInteractionTimeout(ctx, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                choosenIcon = iconPickResult.Result.Emoji;
                await iconPickResult.Result.Message.DeleteReactionAsync(choosenIcon, ctx.User);
            }
            else // If icon is not required
            {
                cancelSource.Dispose();
                cancelSource = new CancellationTokenSource();
                cancelTask.Dispose();
                cancelTask = GetCancellationTask(cancelSource);

                skipTask.Dispose();
                skipTask = WaitForButtonTextSkipResponse(cancelTask, interactivity, messageAfterResponse, ctx.User);
                reactionTask = WaitForButtonIconReactionResponse(cancelTask, interactivity, messageAfterResponse, ctx.User);
                completedTask = await Task.WhenAny(skipTask, reactionTask);
                cancelSource.Cancel();

                // FurmAppClient.Instance.Logger.LogInformation($"\nSkip Task ID = {skipTask.Id}\n"
                //     + $"Reaction Task ID = {reactionTask.Id}\n"
                //     + $"Completed Task: {completedTask.Id}\n");

                // Check skipped response, and check timeout.
                if (completedTask == skipTask && skipTask.Result.TimedOut)
                {
                    await CallInteractionTimeout(ctx, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                if (completedTask == reactionTask && reactionTask != null)
                {
                    choosenIcon = reactionTask.Result.Result.Emoji;
                    reactionTask.Dispose();
                }
            }
            //FurmAppClient.Instance.Logger.LogInformation($"{choosenIcon}");

            // Determine the Custom Button ID
            afterResponseBuilder = new DiscordMessageBuilder(messageAfterResponse);
            afterResponseBuilder.ClearComponents();
            afterResponseBuilder.Content = "```Now provide a Button ID, send an id with message for the button.```";
            webhookMessage = new DiscordWebhookBuilder(afterResponseBuilder);
            messageAfterResponse = await ctx.Interaction.EditOriginalResponseAsync(webhookMessage);

            // Wait for response
            buttonIDInput = await interactivity.WaitForMessageAsync((message) => {
                return ctx.User.Id == message.Author.Id && ctx.Channel.Id == message.Channel.Id;
            }, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

            // Check if timeout.
            if (buttonIDInput.TimedOut)
            {
                await CallInteractionTimeout(ctx, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                return;
            }

            // Assign Button ID value
            buttonID = buttonIDInput.Result.Content;
            await buttonIDInput.Result.DeleteAsync();

            // Final touch, edit the target message
            modifyMessageButton = new DiscordMessageBuilder(targetMessage);
            modifyMessageButton.ClearComponents();
            newButtonForm = new DiscordButtonComponent(
                buttonStyle, buttonID, buttonText,
                emoji: choosenIcon == null ? null : new DiscordComponentEmoji(choosenIcon));
            modifyMessageButton.AddComponents(newButtonForm);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please wait for seconds to edit the message..."));

            // Save to database
            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Adding new button interface data...");
            ButtonInterfaceData buttonInterfaceData = await ButtonInterfaceData.GetData(
                targetMessage.Channel.Guild.Id,
                targetMessage.Channel.Id
            );
            buttonInterfaceData.AddButton($"{targetMessage.Id}", buttonID);
            await buttonInterfaceData.SaveData();
            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Button interface data successfully saved!");

            await targetMessage.ModifyAsync(modifyMessageButton);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(SUCCESS_MESSAGE));
        }
        catch (Exception e)
        {
            FurmAppClient.Instance.Logger.LogError(e, string.Empty);
            await ctx.CreateResponseAsync($"[ERROR] {e.Message}", ephemeral: true);
        }
    }

    public static async Task SetButton(CommandContext ctx, string messageID)
    {
        async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonTextSkipResponse(
            Task cancellationTask, DiscordMessage message, DiscordUser user)
        {
            var waiter = message.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            if (waiter.Id == result.Id)
            {
                await waiter.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                await Task.Delay(100);
            }

            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        async Task<InteractivityResult<DiscordMessage>> WaitForButtonTextTextResponse(
            Task cancellationTask, InteractivityExtension i, DiscordUser user, DiscordChannel channel)
        {
            var waiter = i.WaitForMessageAsync((message) => {
                //FurmAppClient.Instance.Logger.LogInformation($"The User is the same? {user.Id == message.Author.Id}");
                return user.Id == message.Author.Id && channel.Id == message.Channel.Id;
            });

            Task result = await Task.WhenAny(waiter, cancellationTask);
            
            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForButtonIconReactionResponse(
            Task cancellationTask, InteractivityExtension i, DiscordMessage message, DiscordUser user)
        {
            var waiter = i.WaitForReactionAsync(message, ctx.User, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            return waiter.Id == result.Id ? waiter.Result : new() { };
        }

        try
        {
            // Get target message and declare all variables in method.
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            DiscordMessage? targetMessage = null;
            DiscordMessageBuilder messageBuilder;
            DiscordMessage messageResponse;

            InteractivityResult<ComponentInteractionCreateEventArgs> buttonStylePicked;
            InteractivityResult<MessageReactionAddEventArgs> iconPickResult;
            InteractivityResult<DiscordMessage> buttonIDInput;

            CancellationTokenSource cancelSource;
            Task<InteractivityResult<ComponentInteractionCreateEventArgs>> skipTask;
            Task<InteractivityResult<DiscordMessage>> respondTask;
            Task<InteractivityResult<MessageReactionAddEventArgs>>? reactionTask = null;
            Task cancelTask;
            Task completedTask;
            
            DiscordEmoji? choosenIcon = null;
            ButtonStyle buttonStyle = ButtonStyle.Primary;
            string buttonText = string.Empty;
            string buttonID = string.Empty;

            DiscordMessageBuilder modifyMessageButton;
            DiscordButtonComponent newButtonForm;

            bool iconRequired = false;

            // Delete immediately, and then create the mandatory message
            await ctx.Message.DeleteAsync();
            messageResponse = await ctx.Channel.SendMessageAsync("Please wait for a moment...");

            // Convert string to ulong exception handle.
            try
            {
                targetMessage = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
            }
            catch (Exception)
            {
                messageBuilder = new DiscordMessageBuilder()
                    .WithContent("Bad argument inserted to message ID, insert only numbers only.");
                
                await messageResponse.ModifyAsync(messageBuilder);
                return;
            }

            // Check if the target message is not found.
            if (targetMessage == null)
            {
                messageBuilder = new DiscordMessageBuilder()
                    .WithContent("The Message ID target is either Unavailable or Deleted.");
                
                await messageResponse.ModifyAsync(messageBuilder);
                return;
            }

            // Check if the target message is a user message, this must be prevent due to Discord limitation.
            if (!targetMessage.Author.IsBot)
            {
                messageBuilder = new DiscordMessageBuilder()
                    .WithContent("Cannot target the User message.");

                await messageResponse.ModifyAsync(messageBuilder);
                return;
            }

            // Start the process
            messageBuilder = new DiscordMessageBuilder()
                .WithContent("```Choose any Button Style below.```")
                .AddComponents(new DiscordComponent[] {
                    new DiscordButtonComponent(ButtonStyle.Primary, "style1", "Primary"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "style2", "Secondary"),
                    new DiscordButtonComponent(ButtonStyle.Success, "style3", "Success"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "style4", "Danger"),
                });

            // Start with picking Button Style
            messageResponse = await messageResponse.ModifyAsync(messageBuilder);
            buttonStylePicked = await messageResponse.WaitForButtonAsync(ctx.User,
                TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            
            // Timeout response if user didn't pick any button style
            if (buttonStylePicked.TimedOut)
            {
                await CallInteractionTimeout(messageResponse, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                return;
            }

            // Determine which Button used.
            switch (buttonStylePicked.Result.Id)
            {
                case "style1":
                    buttonStyle = ButtonStyle.Primary;
                    break;
                case "style2":
                    buttonStyle = ButtonStyle.Secondary;
                    break;
                case "style3":
                    buttonStyle = ButtonStyle.Success;
                    break;
                case "style4":
                    buttonStyle = ButtonStyle.Danger;
                    break;
            };

            await buttonStylePicked.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            await Task.Delay(100);

            // Edit message to Button Text input content
            messageBuilder = new DiscordMessageBuilder(messageResponse);
            messageBuilder.ClearComponents();
            messageBuilder.Content = "```Send a message to apply Button Text.\n" +
                "Or you can \"Skip\" the Button Text, but the Button Icon is required.```";
            messageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            // var followUp = new DiscordFollowupMessageBuilder()
            //     .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            // followUp.Content = "```Send a message to apply Button Text.\n" +
            //     "Or you can \"Skip\" the Button Text, but the Button Icon is required.```";
            messageResponse = await messageResponse.ModifyAsync(messageBuilder);

            // Wait for response
            cancelSource = new CancellationTokenSource();
            cancelTask = GetCancellationTask(cancelSource);

            skipTask = WaitForButtonTextSkipResponse(cancelTask, messageResponse, ctx.User);
            respondTask = WaitForButtonTextTextResponse(cancelTask, interactivity, ctx.User, ctx.Channel);
            
            completedTask = await Task.WhenAny(skipTask, respondTask);
            cancelSource.Cancel();

            // FurmAppClient.Instance.Logger.LogInformation($"\nSkip Task ID = {skipTask.Id}\n"
            //     + $"Respond Task ID = {respondTask.Id}\n"
            //     + $"Completed Task: {completedTask.Id}\n");

            // Check skipped response.
            if (completedTask == skipTask)
            {
                // Timeout response if not sending any Button Text message.
                if (skipTask.Result.TimedOut)
                {
                    await CallInteractionTimeout(messageResponse, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                iconRequired = true;
            }

            // Check result response.
            if (completedTask == respondTask)
            {
                iconRequired = false;
                buttonText = respondTask.Result.Result.Content;
                await respondTask.Result.Result.DeleteAsync();
                respondTask.Dispose();
            }
            
            // Edit message to Button Icon input content
            messageBuilder = new DiscordMessageBuilder(messageResponse);
            messageBuilder.ClearComponents();
            messageBuilder.Content = $"```[Icon for Button is {(iconRequired ? "REQUIRED" : "OPTIONAL")}]\n" +
                $"{(iconRequired ? "Please send an Icon with reaction for your button."
                : "Would you like to add an Icon? React with an Icon or \"Skip\".")}```";
            if (!iconRequired)
                messageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
            messageResponse = await messageResponse.ModifyAsync(messageBuilder);

            // Wait for user response
            if (iconRequired) // If the icon is required
            {
                iconPickResult = await interactivity.WaitForReactionAsync(
                    messageResponse, ctx.User, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
                
                // Timeout response if not sending any reaction message.
                if (iconPickResult.TimedOut)
                {
                    await CallInteractionTimeout(messageResponse, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                choosenIcon = iconPickResult.Result.Emoji;
                await iconPickResult.Result.Message.DeleteReactionAsync(choosenIcon, ctx.User);
            }
            else // If icon is not required
            {
                cancelSource.Dispose();
                cancelSource = new CancellationTokenSource();
                cancelTask.Dispose();
                cancelTask = GetCancellationTask(cancelSource);

                skipTask.Dispose();
                skipTask = WaitForButtonTextSkipResponse(cancelTask, messageResponse, ctx.User);
                reactionTask = WaitForButtonIconReactionResponse(cancelTask, interactivity, messageResponse, ctx.User);
                completedTask = await Task.WhenAny(skipTask, reactionTask);
                cancelSource.Cancel();

                // FurmAppClient.Instance.Logger.LogInformation($"\nSkip Task ID = {skipTask.Id}\n"
                //     + $"Reaction Task ID = {reactionTask.Id}\n"
                //     + $"Completed Task: {completedTask.Id}\n");

                // Check skipped response, and check timeout.
                if (completedTask == skipTask && skipTask.Result.TimedOut)
                {
                    await CallInteractionTimeout(messageResponse, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                    return;
                }

                if (completedTask == reactionTask && reactionTask != null)
                {
                    choosenIcon = reactionTask.Result.Result.Emoji;
                    reactionTask.Dispose();
                }
            }
            //FurmAppClient.Instance.Logger.LogInformation($"{choosenIcon}");

            // Determine the Custom Button ID
            messageBuilder = new DiscordMessageBuilder(messageResponse);
            messageBuilder.ClearComponents();
            messageBuilder.Content = "```Now provide a Button ID, send an id with message for the button.```";
            messageResponse = await messageResponse.ModifyAsync(messageBuilder);

            // Wait for response
            buttonIDInput = await interactivity.WaitForMessageAsync((message) => {
                return ctx.User.Id == message.Author.Id && ctx.Channel.Id == message.Channel.Id;
            }, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

            // Check if timeout.
            if (buttonIDInput.TimedOut)
            {
                await CallInteractionTimeout(messageResponse, "Timeout! Automatically delete this message in 3 2 1...", 3000);
                return;
            }

            // Assign Button ID value
            buttonID = buttonIDInput.Result.Content;
            await buttonIDInput.Result.DeleteAsync();

            // Final touch, edit the target message
            modifyMessageButton = new DiscordMessageBuilder(targetMessage);
            modifyMessageButton.ClearComponents();
            newButtonForm = new DiscordButtonComponent(
                buttonStyle, buttonID, buttonText,
                emoji: choosenIcon == null ? null : new DiscordComponentEmoji(choosenIcon));
            modifyMessageButton.AddComponents(newButtonForm);

            await messageResponse.ModifyAsync(new DiscordMessageBuilder()
                .WithContent("Please wait for seconds to edit the message..."));

            // Save to database
            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Adding new button interface data...");
            ButtonInterfaceData buttonInterfaceData = await ButtonInterfaceData.GetData(
                targetMessage.Channel.Guild.Id,
                targetMessage.Channel.Id
            );
            buttonInterfaceData.AddButton($"{targetMessage.Id}", buttonID);
            await buttonInterfaceData.SaveData();
            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Button interface data successfully saved!");

            await targetMessage.ModifyAsync(modifyMessageButton);
            await messageResponse.ModifyAsync(new DiscordMessageBuilder().WithContent(SUCCESS_MESSAGE));
        }
        catch (Exception e)
        {
            var response = new DiscordMessageBuilder()
                .WithContent($"[ERROR] {e.Message}");

            FurmAppClient.Instance.Logger.LogError(e, string.Empty);
            await ctx.Channel.SendMessageAsync(response);
        }
    }

    /// <summary>
    /// Receive button interface information on specific message ID.
    /// </summary>
    public static async Task GetButton(InteractionContext ctx, string messageID)
    {
        try
        {
            // Expected a longer time to process the command.
            await ctx.DeferAsync(ephemeral: true);

            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Start getting button interface data from cache...");
            ButtonInterfaceData data = await ButtonInterfaceData.GetData(ctx.Guild.Id, ctx.Channel.Id);
            IReadOnlyDictionary<string, string>? detailData = data[messageID];
            DiscordMessage targetMessage = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
            DiscordWebhookBuilder response = new DiscordWebhookBuilder();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor {
                    IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                    Name = ctx.Client.CurrentUser.Username,
                },
                Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            };

            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Creating message information...");
            IEnumerable<DiscordButtonComponent> buttons;
            bool anyButton = false;
            string desc = string.Empty;
            int buttonCount = 0;
            foreach (var comp in targetMessage.Components)
            {
                buttons = comp.Components.OfType<DiscordButtonComponent>();

                foreach (var btn in buttons)
                {
                    anyButton = true;
                    buttonCount++;
                    desc += $"Button {buttonCount}: `{btn.Emoji} {btn.Label}` (ID: {btn.CustomId}; ";

                    if (detailData == null || !detailData.ContainsKey(btn.CustomId))
                    {
                        desc += $"Form: `UNASSIGNED`)\n";
                        continue;
                    }

                    desc += $"Form: `{detailData[btn.CustomId]}`)\n";
                }
            }

            if (!anyButton)
                desc = "```There are no buttons in this message.```";
            else
                desc = $"There are {buttonCount} Buttons, these are the information (keep the information only for Admin and Mods):\n\n"
                    + $"{desc}";

            embed.Description = desc;
            response.AddEmbed(embed);

            // Send private information message.
            await ctx.Interaction.EditOriginalResponseAsync(response);
        }
        catch (Exception e)
        {
            FurmAppClient.Instance.Logger.LogError(e, string.Empty);
            await ctx.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().WithContent($"[ERROR] {e.Message}"));
        }
    }

    public static async Task RemoveButton(InteractionContext ctx, string messageID, string? buttonID)
    {
        DiscordInteractionResponseBuilder response;
        DiscordMessage? msg = null;

        // Convert string to ulong exception handle.
        try
        {
            msg = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID));
        }
        catch (Exception)
        {
            response = new DiscordInteractionResponseBuilder()
                .WithContent("Bad argument inserted to message ID, insert only numbers only.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
            return;
        }

        // Start the response.
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await Task.Delay(100);

        // Check if the target message is not found.
        if (msg == null)
        {
            response = new DiscordInteractionResponseBuilder()
                .WithContent("The Message ID target is either Unavailable or Deleted.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
            return;
        }

        // Check if there's no button ID input to be handled.
        if (string.IsNullOrEmpty(buttonID))
        {
            buttonID = await PickButtonToBeRemove(ctx, msg);

            // Check if user not input any button ID on this chance.
            if (string.IsNullOrEmpty(buttonID))
            {
                await CallInteractionTimeout(ctx, "Removing Button failed, this message automatically deleted in 3 2 1...", 3000);
                return;
            }
        }

        // Proceed deleting button from message
        DiscordMessageBuilder newMsg = new DiscordMessageBuilder(msg);
        DiscordButtonComponent? targetButton = null;
        List<DiscordActionRowComponent> undeleteComponents = new();
        DiscordActionRowComponent checkedComponent;
        DiscordComponent[] tempComps;
        //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Deleting Button: {buttonID}");
        foreach (var comp in msg.Components)
        {
            //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Checking for component: {comp.CustomId}");
            // Already been found, skip all the checker.
            if (targetButton != null)
            {
                undeleteComponents.Add(comp);
                continue;
            }

            // Find target button.
            targetButton = comp.Components.OfType<DiscordButtonComponent>().First((b) => b.CustomId == buttonID);
            
            // If the target button not found.
            if (targetButton == null)
            {
                undeleteComponents.Add(comp);
                continue;
            }

            // Else then found the target button, proceed deletion by recreating components.
            if (comp.Components.Count - 1 == 0)
                continue;

            tempComps = new DiscordComponent[comp.Components.Count - 1];

            int index = 0;
            foreach (var c in comp.Components)
            {
                if (c.Type == ComponentType.Button && targetButton.CustomId == c.CustomId)
                    continue;

                tempComps[index] = c;
                index++;
            }
            
            checkedComponent = new DiscordActionRowComponent(tempComps);
            undeleteComponents.Add(checkedComponent);
        }
        //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Finish searching for button: {targetButton?.CustomId}");

        // Check if the button has not been found.
        if (targetButton == null)
        {
            await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Button not found, abort the process. You cam now delete this message."));
        }

        // Recreate message components
        //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Rebuilding message...");
        newMsg.ClearComponents();
        foreach (var comp in undeleteComponents)
        {
            foreach (var c in comp.Components)
                newMsg.AddComponents(c);
        }

        //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Modifying target message...");
        await msg.ModifyAsync(newMsg);
        //FurmAppClient.Instance.Logger.LogInformation($"[DEBUG] Successfully modified message.");

        // Finish line.
        await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Successfully deleted the button, you can now delete this message."));
    }

    private static async Task<string> PickButtonToBeRemove(InteractionContext ctx, DiscordMessage message)
    {
        // Get all the button components.
        InteractivityExtension interactivity = ctx.Client.GetInteractivity();
        List<DiscordButtonComponent> buttons = new();

        foreach (var comp in message.Components)
            buttons.AddRange(comp.Components.OfType<DiscordButtonComponent>());

        // Create awaiter to wait for user to pick the button.
        var response = new DiscordWebhookBuilder()
            .WithContent("```Choose the button you want to delete.```")
            .AddComponents(buttons);
        
        var afterResponse = await ctx.Interaction.EditOriginalResponseAsync(response);
        var buttonPick = await afterResponse.WaitForButtonAsync(ctx.User, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Timeout response if user didn't pick any button style
        if (buttonPick.TimedOut) return string.Empty;

        await buttonPick.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
        await Task.Delay(100);

        // Picking process successful, proceed the other process.
        response = new DiscordWebhookBuilder()
            .WithContent("Please wait for a moment to process...");

        await ctx.Interaction.EditOriginalResponseAsync(response);

        // Return the result with custom ID of that button.
        return buttonPick.Result.Id;
    }

    /// <summary>
    /// Call this when timeout interaction.
    /// </summary>
    private static async Task CallInteractionTimeout(InteractionContext ctx, string message, int milisecondsBeforeDeletion)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        await Task.Delay(milisecondsBeforeDeletion);
        await ctx.DeleteResponseAsync();
    }

    /// <summary>
    /// Call this when timeout interaction.
    /// </summary>
    private static async Task CallInteractionTimeout(DiscordMessage awaiterMessage, string message, int milisecondsBeforeDeletion)
    {
        await awaiterMessage.DeleteAsync();
        var m = await awaiterMessage.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(message));
        await Task.Delay(milisecondsBeforeDeletion);
        await m.DeleteAsync();
    }

    /// <summary>
    /// Cancellation tasks, used for timeout or cancel tasks.
    /// </summary>
    private static Task GetCancellationTask(CancellationTokenSource source)
    {
        var tcs = new TaskCompletionSource<object>();
        source.Token.Register(() => tcs.TrySetResult(null));
        return tcs.Task;
    }
}
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;
using FurmAppDBot.Extensions;

namespace FurmAppDBot.Commands;

public class ButtonCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.BUTTON_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Button(CommandContext ctx, string commandName, string messageID, string? buttonID = null)
    {
        // Initial respond with message handler.
        var msgHandler = await ctx.Message.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Selecting the command by name.
                switch (commandName.ToLower())
                {
                    case CMD_CONSTANT.ADD_COMMAND_NAME:
                        await Add(msgHandler, ctx, messageID);
                        break;
                    
                    case CMD_CONSTANT.GET_COMMAND_NAME:
                        await Get(msgHandler, ctx, messageID);
                        break;

                    case CMD_CONSTANT.DELETE_COMMAND_NAME:
                        await Delete(msgHandler, ctx, messageID, buttonID);
                        break;
                }
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    public async Task Add(DiscordMessage msgHandler, CommandContext ctx, string messageID)
    {
        // Search for target message.
        DiscordMessage msgFound;
        try { msgFound = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID)); }
        catch (NotFoundException) // Message not found exception.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process");
            return;
        }
        catch (FormatException) // Wrong input format.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Message ID must be numbers only, abort the process.");
            return;
        }

        // Check if the target message is not the bot itself, this must be prevent due to Discord limitation.
        if (msgFound.Author.Id != ctx.Client.CurrentUser.Id)
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Cannot target this user's message, abort the process.");
            return;
        }

        // Set button process
        await Add(ctx.Client.GetInteractivity(), ctx.User, msgHandler, msgFound);
    }

    public async Task Get(DiscordMessage msgHandler, CommandContext ctx, string messageID)
    {
        // Search for target message.
        DiscordMessage msgFound;
        try { msgFound = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID)); }
        catch (NotFoundException) // Message not found exception.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process");
            return;
        }
        catch (FormatException) // Wrong input format.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Message ID must be numbers only, abort the process.");
            return;
        }

        // Get and send information.
        await Get(new DiscordEmbedBuilder.EmbedAuthor {
            IconUrl = ctx.Client.CurrentUser.AvatarUrl,
            Name = ctx.Client.CurrentUser.Username,
        }, msgHandler, msgFound);
    }

    public async Task Delete(DiscordMessage msgHandler, CommandContext ctx, string messageID, string? buttonID = null)
    {
        // Search for target message.
        DiscordMessage msgFound;
        try { msgFound = await ctx.Channel.GetMessageAsync(ulong.Parse(messageID)); }
        catch (NotFoundException) // Message not found exception.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("The message you are looking for does not exists, abort the process.");
            return;
        }
        catch (FormatException) // Wrong input format.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Message ID must be numbers only, abort the process.");
            return;
        }

        // Check if user did not provide the button ID, then make user choosing it.
        if (string.IsNullOrEmpty(buttonID))
            buttonID = await WaitForChoosingButton(ctx.User, msgHandler, msgFound);
        
        // Check if there's no respond from user.
        if (string.IsNullOrEmpty(buttonID))
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent("Timeout! No Respond from user, abort the process."));
            return;
        }

        // Proceed deleting button from target message.
        await Delete(msgHandler, msgFound, buttonID);
    }

    #endregion
    
    #region Statics

    /// <summary>
    /// Set button on the message.
    /// </summary>
    /// <param name="targetMsg">Target message must be present in one channel</param>
    public static async Task Add(InteractivityExtension interactivity, DiscordUser user, DiscordMessage msgHandler, DiscordMessage targetMsg)
    {
        // Declare requirements
        DiscordEmoji? btnIcon = null;
        ButtonStyle btnStyle = ButtonStyle.Primary;
        string btnText = string.Empty, btnID = string.Empty;

        /// <summary>
        /// Handle waiting for skip action while waiting another action. 
        /// </summary>
        async Task<InteractivityResult<ComponentInteractionCreateEventArgs>?> HandleSkipButton(Task cancellationTask)
        {
            var waiter = msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            if (waiter.Id == result.Id)
                await waiter.Result.Result.Interaction.DeleteOriginalResponseAsync();

            return waiter.Id == result.Id ? waiter.Result : null;
        }

        /// <summary>
        /// Handle waiting for sending message action while waiting another action. 
        /// </summary>
        async Task<InteractivityResult<DiscordMessage>?> HandleTextInput(Task cancellationTask)
        {
            var waiter = interactivity.WaitForMessageAsync((message) => {
                //FurmAppClient.Instance.Logger.LogInformation($"The User is the same? {user.Id == message.Author.Id}");
                return user.Id == message.Author.Id && msgHandler.Channel.Id == message.Channel.Id;
            });
            Task result = await Task.WhenAny(waiter, cancellationTask);
            
            return waiter.Id == result.Id ? waiter.Result : null;
        }

        /// <summary>
        /// Handle waiting for sending reaction action while waiting another action. 
        /// </summary>
        async Task<InteractivityResult<MessageReactionAddEventArgs>?> HandleReactionInput(Task cancellationTask)
        {
            var waiter = interactivity.WaitForReactionAsync(msgHandler, user,
                TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            Task result = await Task.WhenAny(waiter, cancellationTask);

            return waiter.Id == result.Id ? waiter.Result : null;
        }

        // Start the process
        DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder()
            .WithContent("```Choose any Button Style below.```")
            .AddComponents(new DiscordComponent[] {
                new DiscordButtonComponent(ButtonStyle.Primary, "style1", "Primary"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "style2", "Secondary"),
                new DiscordButtonComponent(ButtonStyle.Success, "style3", "Success"),
                new DiscordButtonComponent(ButtonStyle.Danger, "style4", "Danger"),
            });

        // Start with picking Button Style
        msgHandler = await msgHandler.ModifyAsync(msgBuilder);
        var pickButton = await interactivity.WaitForButtonAsync(msgHandler, user,
            TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Timeout response if user didn't pick any button style
        if (pickButton.TimedOut)
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Timeout! Abort the process.");
            return;
        }

        // Determine which button style will be use.
        switch (pickButton.Result.Id)
        {
            case "style1":
                btnStyle = ButtonStyle.Primary;
                break;
            case "style2":
                btnStyle = ButtonStyle.Secondary;
                break;
            case "style3":
                btnStyle = ButtonStyle.Success;
                break;
            case "style4":
                btnStyle = ButtonStyle.Danger;
                break;
        };

        // Finish button respond process
        await pickButton.Result.Interaction.DeleteOriginalResponseAsync();

        // Edit message to Button Text input content
        msgBuilder = new DiscordMessageBuilder(msgHandler);
        msgBuilder.ClearComponents();
        msgBuilder.Content = $"```Style: {btnStyle}\n\n"
            + "Send a message to apply Button Text.\n"
            + "Or you can \"Skip\" the Button Text, but the Button Icon is required.```";
        msgBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
        await msgHandler.ModifyAsync(msgBuilder);

        // Wait for response
        var cancelSource = new CancellationTokenSource();
        var cancelTask = GetCancellationTask(cancelSource);
        var skipTask = HandleSkipButton(cancelTask);
        var respondTask = HandleTextInput(cancelTask);
        var completedTask = await Task.WhenAny(skipTask, respondTask);
        cancelSource.Cancel();

        // Check skipped response.
        bool iconRequired = false;
        if (completedTask == skipTask)
        {
            // Timeout response if not sending any Button Text message.
            if (skipTask.Result != null && skipTask.Result.Value.TimedOut)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Timeout! Abort the process.");
                return;
            }

            // Flagged icon required.
            iconRequired = true;
        }

        // Check result response.
        if (completedTask == respondTask && respondTask.Result != null)
        {
            iconRequired = false;
            btnText = respondTask.Result.Value.Result.Content;
            await respondTask.Result.Value.Result.DeleteAsync();
            respondTask.Dispose();
        }
        
        // Edit message to Button Icon input content
        msgBuilder = new DiscordMessageBuilder(msgHandler);
        msgBuilder.ClearComponents();
        msgBuilder.Content = $"```Style: {btnStyle}\nText on Button: {btnText}\n\n"
            + $"[Icon for Button is {(iconRequired ? "REQUIRED" : "OPTIONAL")}]\n"
            + $"{(iconRequired ? "Please send an Icon with reaction for your button."
            : "Would you like to add an Icon? React with an Icon or \"Skip\".")}```";
        if (!iconRequired) msgBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "skip", "Skip"));
        await msgHandler.ModifyAsync(msgBuilder);

        // Wait for user response
        if (iconRequired) // If the icon is required
        {
            // Receive reaction icon from user
            var pickIcon = await msgHandler.WaitForReactionAsync(user,
                TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            await Task.Delay(100);
            
            // Timeout response if not sending any reaction message.
            if (pickIcon.TimedOut)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Timeout! Abort the process.");
                return;
            }

            // Select icon result
            btnIcon = pickIcon.Result.Emoji;
            await pickIcon.Result.Message.DeleteReactionAsync(btnIcon, user);
        }
        else // If icon is not required
        {
            // Reusing some process variables, waiting for reaction or skip command.
            cancelSource.Dispose();
            cancelSource = new CancellationTokenSource();
            cancelTask.Dispose();
            cancelTask = GetCancellationTask(cancelSource);
            skipTask.Dispose();
            skipTask = HandleSkipButton(cancelTask);
            var reactionTask = HandleReactionInput(cancelTask);
            completedTask.Dispose();
            completedTask = await Task.WhenAny(skipTask, reactionTask);
            cancelSource.Cancel();

            // Check skipped response, and check timeout.
            if (completedTask == skipTask && skipTask.Result != null && skipTask.Result.Value.TimedOut)
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("Timeout! Abort the process.");
                return;
            }

            // Get reaction result.
            if (completedTask == reactionTask && reactionTask.Result != null)
            {
                btnIcon = reactionTask.Result.Value.Result.Emoji;
                reactionTask.Dispose();
            }
        }

        // Determine the Custom Button ID
        msgBuilder = new DiscordMessageBuilder(msgHandler);
        msgBuilder.ClearComponents();
        msgBuilder.Content = $"```Style: {btnStyle}\nText on Button: {btnText}\nIcon: {btnIcon}\n\n"
            + "Now provide a Button ID, send any ID using message for the Button ID.```";
        await msgHandler.ModifyAsync(msgBuilder);

        // Wait for response
        var buttonIDInput = await interactivity.WaitForMessageAsync((message) => {
            return user.Id == message.Author.Id && msgHandler.Channel.Id == message.Channel.Id;
        }, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Check if timeout.
        if (buttonIDInput.TimedOut)
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync("Timeout! Abort the process.");
            return;
        }

        // Assign Button ID value.
        btnID = buttonIDInput.Result.Content;
        await buttonIDInput.Result.DeleteAsync();

        // Handle waiting for editing target message.
        await msgHandler.ModifyAsync("Please wait for seconds to edit the message...");

        // Check if the button with ID has already been defined, then abort the process.
        if (targetMsg.IsComponentWithIDExists(btnID))
        {
            await msgHandler.ModifyAsync($"The button with ID {btnID} has already been defined, process has been aborted!");
            return;
        }

        // Rebuild message and its components.
        var modifiedTargetMsg = new DiscordMessageBuilder(targetMsg);
        var newBtn = new DiscordButtonComponent(btnStyle, btnID, btnText, emoji: btnIcon == null ? null : new DiscordComponentEmoji(btnIcon));
        List<List<DiscordComponent>> comps = new();
        DiscordActionRowComponent[] msgComps = targetMsg.Components.ToArray();
        bool isBtnAdded = false;
        for (int i = 0; i < msgComps.Length; i++)
        {
            // Access one element at a time.
            var rowComp = msgComps[i];
            if (rowComp.Components.Any(c => c is DiscordButtonComponent) && !isBtnAdded)
            {
                comps.Add(new());
                comps[i].AddRange(rowComp.Components);

                // Check for maximal button in one row (Max 5), then create new row.
                if (comps[i].Count >= 5) comps.Add(new() { newBtn });
                else comps[i].Add(newBtn);
                isBtnAdded = true;
                continue;
            }

            // Other components automatically added.
            comps.Add(new(rowComp.Components));
        }

        // Check if button not yet added, then add it as the first button.
        if (!isBtnAdded)
            comps.Add(new() { newBtn });

        // Added all components back to the message.
        modifiedTargetMsg.ClearComponents();
        foreach (var comp in comps)
            modifiedTargetMsg.AddComponents(comp.ToArray());

        // Saving to database.
        InterfaceData buttonInterfaceData;
        try
        {
            // Get data from database.
            buttonInterfaceData = await InterfaceData.GetData(targetMsg.Channel.Guild.Id, targetMsg.Channel.Id);
        }
        catch (InterfaceUnregisteredException)
        {
            // Create new data if not found
            buttonInterfaceData = await InterfaceData.CreateData(targetMsg.Channel.Guild.Id, targetMsg.Channel.Id);
        }
        buttonInterfaceData.AddButton($"{targetMsg.Id}", btnID);
        await buttonInterfaceData.SaveData();
        
        // Final editing steps, then notify finish state.
        await targetMsg.ModifyAsync(modifiedTargetMsg);
        await msgHandler.ModifyAsync(new DiscordMessageBuilder()
            .WithContent("Successfully set button on target message! You can now delete this message.\n"
            + $"Check it out: https://discord.com/channels/{targetMsg.Channel.GuildId}/{targetMsg.ChannelId}/{targetMsg.Id}"));
    }

    /// <summary>
    /// Receive button interface information on specific message ID.
    /// </summary>
    public static async Task Get(DiscordEmbedBuilder.EmbedAuthor botA, DiscordMessage msgHandler, DiscordMessage targetMsg)
    {
        // Initialize message handler.
        DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder().WithContent("Please wait for a moment...");
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
            Author = botA,
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
        };

        // Get all button from message.
        List<DiscordButtonComponent> btns = new();
        foreach (var comp in targetMsg.Components)
            foreach (var c in comp.Components)
                if (c is DiscordButtonComponent)
                    btns.Add((DiscordButtonComponent)c);

        // Check if there's no button in the target message.
        if (btns.Count == 0)
        {
            msgBuilder.Content = string.Empty;
            embed.Description = "```There are no buttons in this message.```";
            msgBuilder.AddEmbed(embed);
            await msgHandler.ModifyAsync(msgBuilder);
            return;
        }

        // Get button information from database.
        InterfaceData? temp = await MainDatabase.Instance.HandleDBProcess<InterfaceData?>(async () => {
            try { return await InterfaceData.GetData(targetMsg.Channel.Guild.Id, targetMsg.ChannelId); }
            catch (InterfaceUnregisteredException) { return null; }
        });

        // Check if it the button interface is found.
        if (temp == null)
        {
            // Give unregistered information.
            embed.Title = "[MESSAGE UNREGISTERED]";
        }

        IReadOnlyDictionary<string, string>? d;
        for (int i = 0; i < btns.Count; i++)
        {
            // Breakdown button detail information.
            embed.Description += $"`{(string.IsNullOrEmpty(btns[i].Label) ? "NO LABEL" : btns[i].Label)}` "
                + $"(Emoji: {(btns[i].Emoji == null ? "`NO EMOJI`" : btns[i].Emoji.Name)}; ID: `{btns[i].CustomId}`; ";

            // Check if it the button interface is found.
            if (temp == null)
            {
                // Set unregistered information.
                embed.Description += $"Form: `UNASSIGNED`)\n";
                continue;
            }
            
            // Check if form information not exists.
            d = temp.GetButtons($"{targetMsg.Id}");
            if (d == null || !d.ContainsKey(btns[i].CustomId) || string.IsNullOrEmpty(d[btns[i].CustomId]))
            {
                embed.Description += $"Form: `UNASSIGNED`)\n";
                continue;
            }
            
            // Get from information.
            embed.Description += $"Form: `{d[btns[i].CustomId]}`)\n";
        }

        // Summarize information, create the message handler.
        msgBuilder.Content = string.Empty;
        embed.Description = $"`Message` => https://discord.com/channels/{targetMsg.Channel.GuildId}/{targetMsg.ChannelId}/{targetMsg.Id}\n"
            + $"There are {btns.Count} Buttons, these are the information "
            + $"(keep the information only for Admin and Mods):\n\n"
            + $"{embed.Description}";
        msgBuilder.Embed = embed;

        // Send information message by editing message handler.
        await msgHandler.ModifyAsync(msgBuilder);
    }

    /// <summary>
    /// Run this if the user did not provide button ID to any button ID paramter when using any command.
    /// </summary>
    /// <param name="user">User must respond to</param>
    /// <param name="msgHandler">Message handler</param>
    /// <param name="targetMsg">The target message that contains button components.</param>
    /// <returns></returns>
    internal static async Task<string> WaitForChoosingButton(DiscordUser user, DiscordMessage msgHandler, DiscordMessage targetMsg)
    {
        // Modify message handler with choosing buttons.
        var msgBuilder = new DiscordMessageBuilder()
            .WithContent("```Choose the button you want to delete.```");

        // Get all the button components and copy it into handler.
        foreach (var comp in targetMsg.Components)
            msgBuilder.AddComponents(comp.Components.OfType<DiscordButtonComponent>());

        // Edit and wait for choosing button respond.
        msgHandler = await msgHandler.ModifyAsync(msgBuilder);
        var pickedBtn = await msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Timeout response if user didn't pick any button style.
        if (pickedBtn.TimedOut) return string.Empty;

        // Set finish the button responded by user.
        await pickedBtn.Result.Interaction.DeleteOriginalResponseAsync();

        // Return the result with custom ID of that button.
        return pickedBtn.Result.Id;
    }

    /// <summary>
    /// Delete target button from message.
    /// Only works if the message owned by this bot, not the other bots or webhooks.
    /// </summary>
    /// <param name="targetMsg">The target message</param>
    /// <param name="buttonID">Target button with ID</param>
    public static async Task Delete(DiscordMessage msgHandler, DiscordMessage targetMsg, string buttonID)
    {
        // Proceed deleting button from message
        DiscordButtonComponent? targetButton = null;
        List<List<DiscordComponent>> undeleteComponents = new();
        var comps = targetMsg.Components.ToArray();
        for (int i = 0; i < comps.Length; i++)
        {
            // Get row component.
            var comp = comps[i];

            // Already been found, skip all the checker.
            if (targetButton != null)
            {
                undeleteComponents.Add(comp.Components.ToList());
                continue;
            }

            // Find target button.
            targetButton = comp.Components.OfType<DiscordButtonComponent>().FirstOrDefault((b) => b.CustomId == buttonID);
            
            // If the target button not found.
            if (targetButton == null)
            {
                undeleteComponents.Add(comp.Components.ToList());
                continue;
            }

            // Check if the component that will be delete is the only one component, then ignore the rest of the process.
            if (comp.Components.Count - 1 == 0) continue;

            // Add empty list.
            undeleteComponents.Add(new());

            // Choose all unpicked components and add it to list.
            foreach (var c in comp.Components)
            {
                // Skip the deleted target button.
                if (c.Type == ComponentType.Button && targetButton.CustomId == c.CustomId) continue;

                // Add the rest of components.
                undeleteComponents[i].Add(c);
            }
        }

        // Check if the button has not been found, then abort the process.
        if (targetButton == null)
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent("The message you are looking for does not exists, abort the process."));
            return;
        }

        // Recreate message components
        var editedMSG = new DiscordMessageBuilder(targetMsg);
        editedMSG.ClearComponents();
        foreach (var comp in undeleteComponents)
            editedMSG.AddComponents(comp.ToArray());

        // Save updates to database.
        try
        {
            InterfaceData data = await InterfaceData.GetData(targetMsg.Channel.Guild.Id, targetMsg.ChannelId);
            data.DeleteButton($"{targetMsg.Id}", targetButton.CustomId);
            await data.SaveData();
        }
        catch (InterfaceUnregisteredException) // When the interface was not registered in the first place.
        {
            // Notify by message handler.
            await msgHandler.ModifyAsync(new DiscordMessageBuilder()
                .WithContent("The message you have selected is not registered in the first place, "
                    + "however the button has been deleted, abort the process."));
            await targetMsg.ModifyAsync(editedMSG);
            return;
        }

        // Edit the target message and notify finish process.
        await targetMsg.ModifyAsync(editedMSG);
        await msgHandler.ModifyAsync(new DiscordMessageBuilder {
            Content = "Successfully deleted the button, you can now delete this message.",
        });
    }

    /// <summary>
    /// Cancellation tasks, used for timeout or cancel tasks.
    /// </summary>
    private static Task GetCancellationTask(CancellationTokenSource source)
    {
        var tcs = new TaskCompletionSource<object?>();
        source.Token.Register(() => tcs.TrySetResult(null));
        return tcs.Task;
    }

    #endregion
}
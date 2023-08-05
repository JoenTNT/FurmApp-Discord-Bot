using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot;

public class DiscordBotWorker : BackgroundService
{
    #region Variables

    private readonly ILogger<DiscordBotWorker> _logger;
    private readonly IConfiguration _config;

    // Clients
    private DiscordClient? _client;
    private DiscordConfiguration? _clientConfig;

    // Commands Extensions
    private InteractivityConfiguration? _interactivityConfig;
    private CommandsNextConfiguration? _commandExtensionConfig;
    private CommandsNextExtension? _commandExtension;
    private SlashCommandsConfiguration? _slashExtensionConfig;
    private SlashCommandsExtension? _slashExtension;

    // Others
    CancellationTokenSource _cancelUpdateStatus = new();
    private bool _isClientConnected = false;

    #endregion

    #region Constructor

    public DiscordBotWorker(ILogger<DiscordBotWorker> _logger, IConfiguration _config)
    {
        this._logger = _logger;
        this._config = _config;
    }

    #endregion

    #region Main

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Notify to console.
        _logger.LogInformation("[DEBUG] Starting the bot...");

        // Client configuration
        _clientConfig = new DiscordConfiguration()
        {
            Token = _config["DBToken"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All | DiscordIntents.AllUnprivileged,
            MessageCacheSize = 0,
        };

        // Create new client.
        _client = new DiscordClient(_clientConfig);

        // Client event handlers
        _client.Ready += ClientReadyCallback;
        _client.MessageCreated += ClientMessageCreatedCallback;
        _client.GuildCreated += ClientGuildCreatedCallback;
        _client.GuildDeleted += ClientGuildDeletedCallback;
        _client.ComponentInteractionCreated += ClientComponentInteractionCreatedCallback;
        _client.ModalSubmitted += ClientModalSubmittedCallback;

        // Interactivity configuration
        _interactivityConfig = new InteractivityConfiguration() { Timeout = TimeSpan.FromMinutes(1d), };
        _client.UseInteractivity(_interactivityConfig);

        // Extension configuration for commands next.
        _commandExtensionConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { CONSTANT.DEFAULT_PREFIX },
            EnableDefaultHelp = false,
        };

        // Register commands.
        _commandExtension = _client.UseCommandsNext(_commandExtensionConfig);
        _commandExtension.RegisterCommands<HelpCommandsModule>();
        _commandExtension.RegisterCommands<ButtonCommandsModule>();
        _commandExtension.RegisterCommands<ConnectCommandsModule>();
        _commandExtension.RegisterCommands<EmbedCommandsModule>();
        _commandExtension.RegisterCommands<FormCommandsModule>();
        _commandExtension.RegisterCommands<QuestionCommandsModule>();
        _commandExtension.RegisterCommands<ContainerCommandsModule>();
        _commandExtension.RegisterCommands<SettingCommandsModule>();
        _commandExtension.RegisterCommands<PingCommandsModule>();
        _commandExtension.RegisterCommands<PurgeCommandsModule>();

        // Extension configuration for slash commands.
        _slashExtensionConfig = new SlashCommandsConfiguration();

        // Register commands.
        _slashExtension = _client.UseSlashCommands(_slashExtensionConfig);
        _slashExtension.RegisterCommands<HelpSlashCommandGroup>();
        _slashExtension.RegisterCommands<ButtonSlashCommandGroup>();
        _slashExtension.RegisterCommands<ConnectSlashCommandGroup>();
        _slashExtension.RegisterCommands<EmbedSlashCommandGroup>();
        _slashExtension.RegisterCommands<FormSlashCommandGroup>();
        _slashExtension.RegisterCommands<QuestionSlashCommandGroup>();
        _slashExtension.RegisterCommands<ContainerSlashCommandGroup>();
        _slashExtension.RegisterCommands<SettingSlashCommandGroup>();
        _slashExtension.RegisterCommands<PingSlashCommandGroup>();
        _slashExtension.RegisterCommands<PurgeSlashCommandGroup>();

        // Initialize singletons, like client and database.
        FurmAppClient.Init(_client, _logger, _config);
        try { await MainDatabase.Init(_logger, _config); }
        catch (DBClientTimeoutException) {
            _logger.LogInformation("[DEBUG] Failed to connect to the database, try again later!");
        }

        // Connect the bot while looping the update status.
        Task statusUpdate = StatusElapse(_client, CONSTANT.STATUS_ELAPSE_TIME_MILISECONDS, _cancelUpdateStatus.Token);
        await _client.ConnectAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client == null) return;

        _cancelUpdateStatus.Cancel();
        await _client.DisconnectAsync();
        _client.Dispose();

        _logger.LogInformation("[DEBUG] The Bot has been stopped.");
    }

    private async Task StatusElapse(DiscordClient client, int everyMiliseconds, CancellationToken cancelToken)
    {
        // Info creation inner functions.
        DiscordActivity GetBotPrefix() => new DiscordActivity($"My Prefix: {CONSTANT.DEFAULT_PREFIX}", ActivityType.ListeningTo);
        DiscordActivity GetServerCount() => new DiscordActivity($"{client.Guilds.Count} Servers", ActivityType.Watching);

        try
        {
            // Waiting for client to connect to the server.
            while (!_isClientConnected)
                await Task.Delay(everyMiliseconds, cancelToken);

            // Infinite loop through all status.
            int index = 0;
            List<Func<DiscordActivity>> statusFunc = new List<Func<DiscordActivity>> { GetBotPrefix, GetServerCount };
            while (!cancelToken.IsCancellationRequested)
            {
                await client.UpdateStatusAsync(statusFunc[index](), UserStatus.Online);
                await Task.Delay(everyMiliseconds, cancelToken);
                index = index + 1 >= statusFunc.Count ? 0 : index + 1;
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("[DEBUG] Status Update has been stopped.");
        }
    }

    private async Task ClientReadyCallback(DiscordClient client, ReadyEventArgs args)
    {
        _logger.LogInformation("[DEBUG] The Bot is Up!");
        _isClientConnected = true;
        await Task.CompletedTask;
    }

    private async Task ClientMessageCreatedCallback(DiscordClient client, MessageCreateEventArgs args)
    {
        // If mentioning the bot clien, then send starter help message.
        if (args.MentionedUsers.Contains(client.CurrentUser))
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor() {
                    IconUrl = client.CurrentUser.AvatarUrl,
                    Name = client.CurrentUser.Username,
                },
                Title = "Hewwo there! :3",
                Description =
                    $"> Anything I can help? try use `{CMD_CONSTANT.HELP_COMMAND_NAME}` command\n" +
                    $"> My prefix is `{CONSTANT.DEFAULT_PREFIX}`, so you can type in `{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.HELP_COMMAND_NAME}`\n" + 
                    "```You can instead use slash command to make your life easier :3```",
                Color = new DiscordColor("FFFFFF"),
            };

            await args.Channel.SendMessageAsync(embed);
        }
    }

    private async Task ClientGuildCreatedCallback(DiscordClient client, GuildCreateEventArgs args)
    {
        await Task.CompletedTask;
    }

    private async Task ClientGuildDeletedCallback(DiscordClient client, GuildDeleteEventArgs args)
    {
        await Task.CompletedTask;
    }

    private async Task ClientComponentInteractionCreatedCallback(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        // Check if interaction is button interaction.
        if (args.Interaction.Data.ComponentType == ComponentType.Button)
        {
            // Check if interface is registered before sending Modal.
            InterfaceData data;
            try { data = await InterfaceData.GetData(args.Guild.Id, args.Channel.Id); }
            catch (InterfaceUnregisteredException) { return; } /* Ignore Interaction. */

            // Check if form is registered before sending Modal, if not exists then ignore it.
            var buttons = data.GetButtons($"{args.Message.Id}");
            if (buttons == null) return;
            if (!buttons.ContainsKey(args.Id)) return;
            if (string.IsNullOrEmpty(buttons[args.Id])) return;
            FormData form;
            try { form = await FormData.GetData(args.Guild.Id, buttons[args.Id]); }
            catch (FormNotFoundException) { return; } /* Ignore Interaction. */

            // Start interaction.
            await args.Interaction.DeferAsync(true);

            // Check if form has no question, send information to user to report this issue.
            if (form.QuestionCount <= 0)
            {
                await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder {
                    Content = "Something went wrong with the form. Please report this issue to Admin/Moderator.",
                });
                return;
            }

            // Notify form fulfilment.
            var notifier = new DiscordWebhookBuilder().WithContent(
                $"The Form is From => https://discord.com/channels/{args.Guild.Id}/{args.Channel.Id}/{args.Message.Id}\n"
                + "Form is Ready! Press the Button Below to start filling.");
            notifier.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "PROCEED!", "Start"));
            var msgHandler = await args.Interaction.EditOriginalResponseAsync(notifier);

            // Wait for start filling.
            var proceedBtn = await client.GetInteractivity().WaitForButtonAsync(msgHandler, args.User,
                TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));
            
            // Check timeout, then abort the process.
            if (proceedBtn.TimedOut)
            {
                try { await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Timeout!")); }
                catch (NotFoundException) { return; } /* Ignore Interaction. */
                return;
            }
            
            // Build and start Modal filling by user.
            DiscordInteractionResponseBuilder modal;
            Dictionary<string, string> answers = new();
            int questionRevealCount = 0, modalPage = 1;
            InteractivityResult<ModalSubmitEventArgs>? submit = null;
            do {
                // Create modal.
                modal = new DiscordInteractionResponseBuilder()
                    .WithTitle("Fill the Form!")
                    .WithCustomId(form.FormID);

                // Reveal 5 questions for each modal.
                for (int i = (modalPage - 1) * 5; i < modalPage * 5; i++)
                {
                    if (i < form.QuestionCount) modal.AddComponents(form[i]);
                    else break;
                }
                
                // Check first submission.
                if (submit == null)
                {
                    // Start filling modal and get submissions from it.
                    await proceedBtn.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                }
                else
                {
                    // Start filling modal and get submissions from it.
                    await submit.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                }

                // Wait for submission.
                submit = await client.GetInteractivity().WaitForModalAsync(form.FormID, args.User,
                    TimeSpan.FromSeconds(CONSTANT.FILLING_FORM_IN_SECONDS_DEFAULT_TIMEOUT));
                
                // Check timeout or cancelled.
                if (submit.Value.TimedOut)
                {
                    // Notify cancellation or timeout.
                    try { await msgHandler.DeleteAsync(); }
                    catch (NotFoundException) { /* Ignore Interaction */ }
                    return;
                }

                // Put submissions to temporary list of data.
                foreach (var ans in submit.Value.Result.Values)
                    answers.Add(ans.Key, ans.Value);

                // Next question reveal.
                questionRevealCount += 5;
                modalPage++;
            } while (questionRevealCount < form.QuestionCount);

            // Notify submission.
            await submit.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

            try { await proceedBtn.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder { Content = "Processing...", }); }
            catch (NotFoundException) { /* Ignore if message handler has been deleted. */ }

            // Get channel category as container of submission.
            ulong ccID = await SettingCommandsModule.GetChannelCategory(await args.Guild.GetMemberAsync(client.CurrentUser.Id), args.Guild);
            DiscordChannel channelCat = args.Guild.GetChannel(ccID);

            // Check if channel for specific form ID submission not yet created.
            DiscordChannel? formSubmissionChannel = args.Guild.Channels.FirstOrDefault(c => c.Value.Name == form.ChannelName).Value;
            if (formSubmissionChannel == null)
            {
                // Create submission channel.
                formSubmissionChannel = await args.Guild.CreateChannelAsync(form.ChannelName, ChannelType.Text, channelCat);

                // Making sure the bot has permission to send message to channel.
                await formSubmissionChannel.AddOverwriteAsync(await args.Guild.GetMemberAsync(client.CurrentUser.Id),
                    Permissions.AccessChannels | Permissions.ReadMessageHistory);
                
                // Don't let anyone see this channel.
                await formSubmissionChannel.AddOverwriteAsync(args.Guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);
            }

            // Send to target channel.
            DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder()
                .WithContent($"Submitted by {args.User.Mention}");
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
                Title = $"Submission [Form with ID: {form.FormID}] by {args.User.Username}",
                Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            };
            TextInputComponent question;
            for (int i = 0; i < form.QuestionCount; i++)
            {
                question = form[i];
                embed.AddField($"**{question.Label}**", $"{answers[question.CustomId]}", false);
            }

            // Send message to submission channel.
            msgBuilder.Embed = embed;
            await formSubmissionChannel.SendMessageAsync(msgBuilder);

            // Send notification to user.
            try { await proceedBtn.Result.Interaction.EditOriginalResponseAsync(
                new DiscordWebhookBuilder { Content = "Your submission has been sent.", }); }
            catch (NotFoundException) { /* Ignore if message handler has been deleted. */ }
        }
    }

    private async Task ClientModalSubmittedCallback(DiscordClient sender, ModalSubmitEventArgs args)
    {
        await Task.CompletedTask;
    }

    #endregion
}

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;
using FurmAppDBot.Commands;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot;

public class Worker : BackgroundService
{
    #region Variables

    private readonly ILogger<Worker> _logger;
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

    public Worker(ILogger<Worker> _logger, IConfiguration _config)
    {
        this._logger = _logger;
        this._config = _config;
    }

    #endregion

    #region Main

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the bot...");

        // Client configuration
        _clientConfig = new DiscordConfiguration()
        {
            Token = _config["DBToken"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All | DiscordIntents.AllUnprivileged,
        };

        _client = new DiscordClient(_clientConfig);

        // Client event handlers
        _client.Ready += ClientReadyCallback;
        _client.MessageCreated += ClientMessageCreatedCallback;
        _client.GuildCreated += ClientGuildCreatedCallback;
        _client.GuildDeleted += ClientGuildDeletedCallback;
        // _client.InteractionCreated += ClientInteractionCreatedCallback; // TODO: Other interaction case.
        _client.ComponentInteractionCreated += ClientComponentInteractionCreatedCallback;

        // Interactivity configuration
        _interactivityConfig = new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromMinutes(1d),
        };

        _client.UseInteractivity(_interactivityConfig);

        // Extension configuration
        _commandExtensionConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { CONSTANT.DEFAULT_PREFIX },
            EnableDefaultHelp = false,
        };

        _commandExtension = _client.UseCommandsNext(_commandExtensionConfig);
        _commandExtension.RegisterCommands<CommandNextFunctions>();
        _commandExtension.RegisterCommands<HelpCommandsModule>();
        _commandExtension.RegisterCommands<ButtonCommandsModule>();
        _commandExtension.RegisterCommands<ConnectCommandsModule>();
        _commandExtension.RegisterCommands<EmbedCommandsModule>();
        _commandExtension.RegisterCommands<FormCommandsModule>();
        _commandExtension.RegisterCommands<QuestionCommandsModule>();
        _commandExtension.RegisterCommands<ContainerCommandsModule>();
        _commandExtension.RegisterCommands<SettingCommandsModule>();

        _slashExtensionConfig = new SlashCommandsConfiguration();

        _slashExtension = _client.UseSlashCommands(_slashExtensionConfig);
        _slashExtension.RegisterCommands<SlashCommandFunctions>();
        _slashExtension.RegisterCommands<HelpSlashCommandGroup>();
        _slashExtension.RegisterCommands<ButtonSlashCommandGroup>();
        _slashExtension.RegisterCommands<ConnectSlashCommandGroup>();
        _slashExtension.RegisterCommands<EmbedSlashCommandGroup>();
        _slashExtension.RegisterCommands<FormSlashCommandGroup>();
        _slashExtension.RegisterCommands<QuestionSlashCommandGroup>();
        _slashExtension.RegisterCommands<ContainerSlashCommandGroup>();
        _slashExtension.RegisterCommands<SettingSlashCommandGroup>();

        // Initialize singletons
        FurmAppClient.Init(_client, _logger, _config);
        try
        {
            await MainDatabase.Init(_logger, _config);
        }
        catch (DBClientTimeoutException)
        {
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
        DiscordActivity GetBotPrefix() => new DiscordActivity($"My Prefix: {CONSTANT.DEFAULT_PREFIX}", ActivityType.ListeningTo);
        DiscordActivity GetServerCount() => new DiscordActivity($"{client.Guilds.Count} Servers", ActivityType.Watching);

        try
        {
            while (!_isClientConnected)
                await Task.Delay(everyMiliseconds, cancelToken);

            int index = 0;
            List<Func<DiscordActivity>> statusFunc = new List<Func<DiscordActivity>> { GetBotPrefix, GetServerCount };
            while (!cancelToken.IsCancellationRequested)
            {
                //_logger.LogInformation("[DEBUG] Updating status...");
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

    // TODO: Other interaction case.
    // private async Task ClientInteractionCreatedCallback(DiscordClient client, InteractionCreateEventArgs args)
    // {
    //     // Check if interaction component was called by user.
    //     if (args.Interaction.Type == InteractionType.Component)
    //     {
    //         // Check interaction is from button component.
            
    //         _logger.LogInformation($"Interact with Component: {args.Interaction.Id}");
    //     }

    //     await Task.CompletedTask;
    // }

    private async Task ClientComponentInteractionCreatedCallback(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        // Check if interaction is button interaction.
        if (args.Interaction.Data.ComponentType == ComponentType.Button)
        {
            // Start interaction.
            await args.Interaction.DeferAsync();

            // Check if interface is registered before sending Modal.
            InterfaceData data;
            try { data = await InterfaceData.GetData(args.Guild.Id, args.Channel.Id); }
            catch (InterfaceUnregisteredException) { return; } /* Ignore Interaction. */

            // Check if form is registered before sending Modal, if not exists then ignore it.
            var buttons = data.GetButtons($"{args.Message.Id}");
            if (buttons == null) return;
            if (string.IsNullOrEmpty(buttons[args.Id])) return;
            FormData form;
            try { form = await FormData.GetData(args.Guild.Id, buttons[args.Id]); }
            catch (FormNotFoundException) { return; } /* Ignore Interaction. */

            // Start interaction.
            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            await Task.Delay(100);

            // Check if form has no question, send information to user to report this issue.
            if (form.QuestionCount <= 0)
            {
                // TODO: Send info to user to report issue.
                return;
            }
            
            // Start Modal filling by user.
            // TODO: Create modal for user to fill in.
        }
    }

    #endregion
}

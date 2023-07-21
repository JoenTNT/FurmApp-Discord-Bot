using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FurmAppDBot.Clients;

namespace FurmAppDBot.Commands;

public class HelpCommandsModule : BaseCommandModule
{
    #region Variables

    public const string MAIN_HELP_PAGE_KEY = "main";

    #endregion

    #region Properties

    private static DiscordButtonComponent ButtonCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.BUTTON_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("▶️"));
    
    private static DiscordButtonComponent ConnectCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.CONNECT_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("⛓️"));

    private static DiscordButtonComponent EmbedCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.EMBED_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("🖼️"));

    private static DiscordButtonComponent FormCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.FORM_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("📝"));

    private static DiscordButtonComponent HelpCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.HELP_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("❓"));
    
    private static DiscordButtonComponent PingCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.PING_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("❗"));
    
    private static DiscordButtonComponent PurgeCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.PURGE_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("🗑️"));

    private static DiscordButtonComponent QuestionCommandBtnComp => new DiscordButtonComponent(
        ButtonStyle.Primary, CMD_CONSTANT.QUESTION_COMMAND_NAME, null, emoji: new DiscordComponentEmoji("☁️"));

    private static DiscordButtonComponent MainMenuBtnComp => new DiscordButtonComponent(
        ButtonStyle.Secondary, MAIN_HELP_PAGE_KEY, "Go to Main");

    #endregion

    #region Main

    [Command(CMD_CONSTANT.HELP_COMMAND_NAME)]
    public async Task Help(CommandContext ctx, string? commandName = null)
    {
        // Intialize message handler.
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            // Set initial command name to main if user didn't input any.
            if (string.IsNullOrEmpty(commandName)) commandName = MAIN_HELP_PAGE_KEY;

            // Start by choosing command.
            await ChooseCommand(ctx.Client, ctx.User, msgHandler, commandName);
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    #endregion

    #region Statics

    public static async Task ChooseCommand(DiscordClient client, DiscordUser user, DiscordMessage msgHandler, string? commandName = null)
    {
        do {
            // Timeout happens, then remove all components from message handler.
            if (string.IsNullOrEmpty(commandName))
            {
                var msgBuilder = new DiscordMessageBuilder(msgHandler);
                msgBuilder.ClearComponents();
                await msgHandler.ModifyAsync(msgBuilder);
                break;
            }

            // Check for specific help.
            switch (commandName.ToLower())
            {
                case MAIN_HELP_PAGE_KEY:
                    commandName = await Main(user, msgHandler);
                    continue;

                case CMD_CONSTANT.BUTTON_COMMAND_NAME:
                    continue;
                
                case CMD_CONSTANT.CONNECT_COMMAND_NAME:
                    continue;

                case CMD_CONSTANT.EMBED_COMMAND_NAME:
                    continue;
                
                case CMD_CONSTANT.FORM_COMMAND_NAME:
                    continue;

                case CMD_CONSTANT.HELP_COMMAND_NAME:
                    continue;
                
                case CMD_CONSTANT.PING_COMMAND_NAME:
                    commandName = await Ping(client, user, msgHandler);
                    continue;

                case CMD_CONSTANT.PURGE_COMMAND_NAME:
                    continue;
                
                case CMD_CONSTANT.QUESTION_COMMAND_NAME:
                    continue;
            }
        } while (!string.IsNullOrEmpty(commandName));
    }

    public static async Task<string?> Main(DiscordUser user, DiscordMessage msgHandler)
    {
        // Create initial embed.
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder() {
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            Title = "Need Help?",
        };
        embed.Description = $"Here's the summary list of main commands (Prefix: `{CONSTANT.DEFAULT_PREFIX}`):\n"
            + $"> 1. ▶️ **Button** Command `=>` `{CMD_CONSTANT.BUTTON_COMMAND_NAME}`\n"
            + $"> 2. ⛓️ **Connect** Command `=>` `{CMD_CONSTANT.CONNECT_COMMAND_NAME}`\n"
            + $"> 3. 🖼️ **Embed** Command `=>` `{CMD_CONSTANT.EMBED_COMMAND_NAME}`\n"
            + $"> 4. 📝 **Form** Command `=>` `{CMD_CONSTANT.FORM_COMMAND_NAME}`\n"
            + $"> 5. ❓ **Help** Command `=>` `{CMD_CONSTANT.HELP_COMMAND_NAME}`\n"
            + $"> 6. ❗ **Ping** Command `=>` `{CMD_CONSTANT.PING_COMMAND_NAME}`\n"
            + $"> 7. 🗑️ **Purge** Command `=>` `{CMD_CONSTANT.PURGE_COMMAND_NAME}`\n"
            + $"> 8. ☁️ **Question** Command `=>` `{CMD_CONSTANT.QUESTION_COMMAND_NAME}`\n"
            + $"`Choose a command for more help detail.`";

        // Create interaction message.
        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.Embed = embed;
        msgBuilder.AddComponents(new DiscordComponent[] { // First row
            ButtonCommandBtnComp, ConnectCommandBtnComp, EmbedCommandBtnComp, FormCommandBtnComp, HelpCommandBtnComp
        });
        msgBuilder.AddComponents(new DiscordComponent[] { // Second row
            PingCommandBtnComp, PurgeCommandBtnComp, QuestionCommandBtnComp,
        });

        // Edit help message interaction.
        msgHandler = await msgHandler.ModifyAsync(msgBuilder);

        // Wait for picking command detail.
        var pickBtn = await msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Check if interaction timeout, then ignore it.
        if (pickBtn.TimedOut) return null;

        // Respond the button.
        await pickBtn.Result.Interaction.DeleteOriginalResponseAsync();

        // Return command name to change page.
        return pickBtn.Result.Id;
    }

    public static async Task<string?> Ping(DiscordClient client, DiscordUser user, DiscordMessage msgHandler)
    {
        // Create initial embed.
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder() {
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
            Title = "~ Ping Command Help ~",
        };
        var pingMS = client.Ping;
        embed.Description = $"This is a Ping Command, used to check how much latency am I?\n"
            + "This does made me thinking, am I Okay?...\n"
            + $"Checking my Heartbeat... Beep Boop... Pong! {pingMS}ms\n\n"
            + $"`{(pingMS < 100 ? "I am Speeeeeeeeed! :D" : (pingMS > 1000 ? "I don't feel well today :(" : "~ Oh! I'm Okay! :) ~"))}`";
        embed.AddField("💻 Command Examples",
            $"```{CONSTANT.DEFAULT_PREFIX}{CMD_CONSTANT.HELP_COMMAND_NAME}```");
        
        // Create interaction message.
        var msgBuilder = new DiscordMessageBuilder(msgHandler);
        msgBuilder.ClearComponents();
        msgBuilder.Embed = embed;
        msgBuilder.AddComponents(MainMenuBtnComp);
        
        // Edit help message interaction.
        msgHandler = await msgHandler.ModifyAsync(msgBuilder);

        // Wait for picking command detail.
        var pickBtn = await msgHandler.WaitForButtonAsync(user, TimeSpan.FromSeconds(CMD_CONSTANT.TIMEOUT_SECONDS_DEFAULT));

        // Check if interaction timeout, then ignore it.
        if (pickBtn.TimedOut) return null;

        // Respond the button.
        await pickBtn.Result.Interaction.DeleteOriginalResponseAsync();

        // Return command name to change page.
        return pickBtn.Result.Id;
    }

    #endregion
}
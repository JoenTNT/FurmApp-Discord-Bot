using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace FurmAppDBot.Commands;

public static class PurgeCommand
{
    public static async Task Purge(InteractionContext ctx, string purgeAmount)
    {
        try
        {
            var messages = await ctx.Channel.GetMessagesAsync(int.Parse(purgeAmount));
            int amount = messages.Count;

            var response = new DiscordWebhookBuilder().WithContent($"Purge successfully done! (Purged {amount} messages)\n"
                + "This message will automatically deleted in 3 seconds... 3 2 1 ...");

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            // Purging process
            foreach (var m in messages)
                await m.DeleteAsync();

            await ctx.EditResponseAsync(response);
            await Task.Delay(3000); // Wait for 3 seconds.
            await ctx.DeleteResponseAsync();
        }
        catch (Exception)
        {
            var response = new DiscordInteractionResponseBuilder().WithContent("[ERROR] Invalid input, the purge amount must be number.");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }

    public static async Task Purge(CommandContext ctx, string purgeAmount)
    {
        try
        {
            var messages = await ctx.Channel.GetMessagesAsync(int.Parse(purgeAmount));
            int amount = messages.Count;

            var response = new DiscordMessageBuilder().WithContent($"Purge successfully done! (Purged {amount} messages)\n"
                + "This message will automatically deleted in 3 seconds... 3 2 1 ...");

            var initialMessage = await ctx.Channel.SendMessageAsync("Purging in progress...");
            
            // Purging process
            foreach (var m in messages)
                await m.DeleteAsync();

            await initialMessage.ModifyAsync(response);
            await Task.Delay(3000); // Wait for 3 seconds.
            await initialMessage.DeleteAsync();
        }
        catch (Exception)
        {
            var response = new DiscordMessageBuilder().WithContent("[ERROR] Invalid input, the purge amount must be number.");

            await ctx.Channel.SendMessageAsync(response);
        }
    }
}
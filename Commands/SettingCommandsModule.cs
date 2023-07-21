using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using FurmAppDBot.Clients;
using FurmAppDBot.Databases;
using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Commands;

// TODO: Settings command.
public class SettingCommandsModule : BaseCommandModule
{
    #region Main

    [Command(CMD_CONSTANT.SETTING_COMMAND_NAME)]
    [RequirePermissions(Permissions.ManageGuild)]
    public async Task Setting(CommandContext ctx, string optionName, params string[] inputs)
    {
        // Handle a long process when interacting
        var msgHandler = await ctx.RespondAsync("Please wait for a moment...");

        try
        {
            try
            {
                // Check if user is using command in server as member.
                if (ctx.Member == null)
                {
                    // Notify message handler.
                    await msgHandler.ModifyAsync("This command only can be used in server, abort the process.");
                    return;
                }

                // Selecting the command by name.
                switch (optionName.ToLower())
                {
                    case CMD_CONSTANT.CATEGORY_COMMAND_NAME:
                        // Start setting category.
                        await SetCategory(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), msgHandler, ctx.Member,
                            inputs.Length <= 0 ? null : ulong.Parse(inputs[0]));
                        break;

                    case CMD_CONSTANT.INFO_COMMAND_NAME:
                        // Send information to user in server.
                        await Info(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id),
                            new DiscordEmbedBuilder.EmbedAuthor {
                                Name = ctx.Client.CurrentUser.Username,
                                IconUrl = ctx.Client.CurrentUser.AvatarUrl,
                            }, msgHandler, ctx.Member);
                        break;
                }
            }
            catch (DBClientTimeoutException) // When database connection has timed out.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync("```Request Time Out, please try again later!```");
            }
            catch (FormatException) // Wrong input format.
            {
                // Notify by message handler.
                await msgHandler.ModifyAsync($"Bad argument detected, some insert parameters are incorrect, abort the process.");
            }
        }
        catch (NotFoundException) { /* Ignore/abort process if user deleted any message handler */ }
    }

    #endregion

    #region Statics

    public static async Task SetCategory(DiscordMember botAsMember, DiscordMessage msgHandler, DiscordMember member, ulong? ccID = null)
    {
        // Get all channels of the guild.
        var guild = msgHandler.Channel.Guild;

        // Check if user did not input channel category ID.
        DiscordChannel? channelCat;
        if (ccID == null)
        {
            // Check if channel category had been set in database, so this process can be ignored.
            ulong? chcatInDB = await SettingsDataMethods.GetCategoryAsContainer(guild);
            if (chcatInDB != null)
            {
                // Find channel category.
                channelCat = guild.Channels.FirstOrDefault(c => c.Value.Id == chcatInDB).Value;

                // Check if the channel category is presented in the server, then ignore the process.
                if (channelCat != null)
                {
                    // Notify already registered.
                    await msgHandler.ModifyAsync("Channel Category as a container had already been set, abort the process.");
                    return;
                }
            }

            // Find the default channel category by name.
            channelCat = guild.Channels.FirstOrDefault(c => c.Value.Name == $"{guild.Name} Submissions").Value;

            // Check if default channel name has been found and it is already registered in database.
            if (channelCat != null && chcatInDB != null && channelCat.Id == chcatInDB)
            {
                // Notify message handler to ignore process.
                await msgHandler.ModifyAsync("Default channel category already presented, abort the process.");
                return;
            }
            
            // Create default private channel category as container.
            channelCat = await CreateDefaultPrivateChannel(botAsMember, guild, member);
            ccID = channelCat.Id;

            // Set channel category as submission container.
            await SettingsDataMethods.SetCategoryAsContainer(guild, (ulong)ccID);
            
            // Notify successful process.
            await msgHandler.ModifyAsync("Category has been created and saved!");
            return;
        }

        // Check if user did not input CCID, both condition finds the channel category.
        channelCat = guild.Channels.FirstOrDefault(c => c.Value.Id == ccID && c.Value.IsCategory).Value;
        bool isCategoryMade = false;

        // Check if channel category not found.
        if (channelCat == null)
        {
            // Notify channel not found.
            await msgHandler.ModifyAsync("Channel may not be exists or is not channel category, abort the process.");
            return;
        }

        // Set channel category as submission container.
        await SettingsDataMethods.SetCategoryAsContainer(guild, (ulong)ccID);
        
        // Notify successful process.
        await msgHandler.ModifyAsync((isCategoryMade ? "Category has been created!\n" : string.Empty)
            + $"Successfully setting channel category to `{channelCat.Name}`.");
    }

    public static async Task Info(DiscordMember botAsMember, DiscordEmbedBuilder.EmbedAuthor author, DiscordMessage msgHandler, DiscordMember member)
    {
        // Create initial message builder and embed.
        DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
            Title = "~ Server Settings Information ~",
            Description = $"My Prefix for this server is `{CONSTANT.DEFAULT_PREFIX}`",
            Color = new DiscordColor(CMD_CONSTANT.EMBED_HEX_COLOR_DEFAULT),
        };

        // Get guild data.
        var guild = msgHandler.Channel.Guild;

        // Get channel category information.
        ulong ccID = await GetChannelCategory(botAsMember, guild, member);
        
        // Add informations to embed.
        embed.AddField("**Chnl Category**", $"<#{ccID}>", true);

        // Send information.
        msgBuilder.Embed = embed;
        await msgHandler.ModifyAsync(msgBuilder);
    }

    public static async Task<ulong> GetChannelCategory(DiscordMember botAsMember, DiscordGuild guild, DiscordMember? member = null)
    {
        // Getting channel category from database.
        ulong? ccID = await SettingsDataMethods.GetCategoryAsContainer(guild);
        DiscordChannel? channelCat = ccID == null ? null : guild.Channels[(ulong)ccID];

        // Check if channel category not exists.
        if (ccID == null || channelCat == null)
        {
            // Find default channel category
            channelCat = guild.Channels.FirstOrDefault(c => c.Value.Name == $"{guild.Name} Submissions").Value;

            // Check default channel category not exists, then create one.
            if (channelCat == null)
                channelCat = await CreateDefaultPrivateChannel(botAsMember, guild, member);

            // Get default channel category ID.
            ccID = channelCat.Id;

            // Set channel category as submission container.
            await SettingsDataMethods.SetCategoryAsContainer(guild, (ulong)ccID);
        }

        // Return channel category ID.
        return (ulong)ccID;
    }

    private static async Task<DiscordChannel> CreateDefaultPrivateChannel(DiscordMember botAsMember, DiscordGuild guild, DiscordMember? member = null)
    {
        // Create new private channel category.
        var channelCat = await guild.CreateChannelCategoryAsync($"{guild.Name} Submissions");
        await channelCat.AddOverwriteAsync(botAsMember, Permissions.AccessChannels | Permissions.ReadMessageHistory);
        await channelCat.AddOverwriteAsync(guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);

        // If a member that has high role defined.
        if (member != null)
        {
            // Select all managerial permissions
            var amRoles = member.Roles.Where(r => PermissionLevel.Allowed == r.CheckPermission(Permissions.ManageGuild));

            // Overwrite channel with all roles permissions.
            foreach (var mr in amRoles)
                await channelCat.AddOverwriteAsync(mr, Permissions.AccessChannels | Permissions.ReadMessageHistory);
        }

        // Return created channel.
        return channelCat;
    }

    #endregion
}
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace FurmAppDBot.Commands.Attributes;

public class DeveloperOnlyAttribute : CheckBaseAttribute
{
    // The name is JoenTNT, nice to meet you!
    private const ulong OWNER_ID = 327984798398283778;

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var isDeveloper = ctx.User.Id == OWNER_ID;
        return Task.FromResult(isDeveloper);
    }
}
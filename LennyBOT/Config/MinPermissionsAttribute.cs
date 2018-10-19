// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Config
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Discord.Commands;
    using Discord.WebSocket;

    public enum AccessLevel
    {
        // ReSharper disable StyleCop.SA1602
        Blocked,
        User,
        ServerMod,
        ServerAdmin,
        ServerOwner,
        BotOwner
    }

    /// <summary>
    /// Set the minimum permission required to use a module or command
    /// similar to how MinPermissions works in Discord.Net 0.9.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private readonly AccessLevel level;

        public MinPermissionsAttribute(AccessLevel level)
        {
            this.level = level;
        }

        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            // If the user's access level is greater than the required level, return success.
            return Task.FromResult(access >= this.level ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Insufficient permissions."));
        }

        private static AccessLevel GetPermission(ICommandContext c)
        {
            // Prevent other bots from executing commands.
            if (c.User.IsBot)
            {
                return AccessLevel.Blocked;
            }

            // Give configured owners special access.
            if (Configuration.Load().Owners.Contains(c.User.Id))
            {
                return AccessLevel.BotOwner;
            }

            // Check if the context is in a guild.
            if (!(c.User is SocketGuildUser user))
            {
                return AccessLevel.Blocked;
            }

            if (c.Guild.OwnerId == user.Id)
            {
                // Check if the user is the guild owner.
                return AccessLevel.ServerOwner;
            }

            if (user.GuildPermissions.Administrator)
            {
                // Check if the user has the administrator permission.
                return AccessLevel.ServerAdmin;
            }

            if (user.GuildPermissions.ManageMessages || user.GuildPermissions.BanMembers
                || user.GuildPermissions.KickMembers)
            {
                // Check if the user can ban, kick, or manage messages.
                return AccessLevel.ServerMod;
            }

            // If nothing else, return a default permission.
            return AccessLevel.User;
        }
    }
}
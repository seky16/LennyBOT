// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
// ReSharper disable StyleCop.SA1126
namespace LennyBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Extensions;

    [Name("Info")]
    public class InfoModule : LennyBase
    {
        private readonly CommandService service;

        private readonly Stopwatch watch;

        public InfoModule(CommandService service)
        {
            this.service = service;
            this.watch = new Stopwatch();
            this.watch.Start();
        }

        [Command("info")]
        [Alias("about", "stats")]
        [Remarks("Get info about bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task InfoCmdAsync()
        {
            var app = await this.Context.Client.GetApplicationInfoAsync();

            await this.ReplyAsync(
                $"{Format.Bold("Info")}\n"
                + $"- Author: {app.Owner} ({app.Owner.Id})\n"
                + $"- Library: Discord.Net ({DiscordConfig.Version})\n"
                + $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} "
                + $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n"
                + $"- Uptime: {GetUptime()}\n\n"

                + $"{Format.Bold("Stats")}\n"
                + $"- Heap Size: {GetHeapSize()}MiB\n"
                + $"- Guilds: {this.Context.Client.Guilds.Count}\n"
                + $"- Channels: {this.Context.Client.Guilds.Sum(g => g.Channels.Count)}\n"
                + $"- Users: {this.Context.Client.Guilds.Sum(g => g.Users.Count)}\n")
                ;
        }

        /*[Command("debug")]
        [Remarks("Get ids of guild, channel, user")]
        [MinPermissions(AccessLevel.User)]
        public Task DebugCmdAsync()
        {
            var serverId = (this.Context.Channel as IGuildChannel)?.GuildId.ToString() ?? "n/a";
            var response =
                   "```json"
                 + $"  Guild ID: {serverId}\n"
                 + $"Channel ID: {this.Context.Channel.Id}\n"
                 + $"   User ID: {this.Context.User.Id}```";
            return this.ReplyAsync(response);
        }*/

        [Command("help")]
        [Remarks("Get help about command")]
        [MinPermissions(AccessLevel.User)]
        public async Task HelpCmdAsync([Remainder] string command = null)
        {
            command = command ?? string.Empty;
            if (command == string.Empty)
            {
                var prefix = Configuration.Load().Prefix.ToString();
                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Description = "These are the commands you can use:"
                };

                foreach (var module in this.service.Modules)
                {
                    var description = string.Empty;

                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(this.Context);
                        if (!result.IsSuccess)
                        {
                            continue;
                        }

                        var toAdd = prefix + string.Join($", {prefix}", cmd.Aliases) + "\n";
                        if (description.Contains(toAdd))
                        {
                            continue;
                        }

                        description += toAdd;
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(
                            x =>
                                {
                                    x.Name = module.Name;
                                    x.Value = description;
                                    x.IsInline = false;
                                });
                    }
                }

                await this.ReplyAsync(string.Empty, false, builder.Build());
            }
            else
            {
                var result = this.service.Search(this.Context, command);

                if (!result.IsSuccess)
                {
                    await this.ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;
                    var parameters = string.Join(", ", cmd.Parameters.Select(p => p.Name));
                    if (parameters != string.Empty)
                    {
                        parameters = $"Parameters: {parameters}\n";
                    }

                    builder.AddField(
                        x =>
                            {
                                x.Name = string.Join(", ", cmd.Aliases);
                                x.Value = $"{parameters}"
                                          + $"Description: {cmd.Remarks}\n";
                                x.IsInline = false;
                            });
                }

                await this.ReplyAsync(string.Empty, false, builder.Build());
            }
        }

        [Command("ping"), Alias("latency")]
        [Remarks("Get info about bot's ping/latency")]
        [MinPermissions(AccessLevel.User)]
        public Task PingCmdAsync()
        {
            this.watch.Stop();
            var elapsed = this.watch.ElapsedMilliseconds;
            var ping = this.Context.Client.Latency.ToString();
            var embedB = new EmbedBuilder().WithTitle("Pong!").WithDescription($"Execution: {elapsed} ms").WithFooter($"Ping: {ping} ms");
            return this.ReplyAsync(string.Empty, false, embedB.Build());
        }

        [Command("userinfo"), Alias("user", "u", "whois")]
        [Remarks("Get info about user")]
        [MinPermissions(AccessLevel.User)]
        public Task UserInfoCmdAsync(SocketGuildUser user = null)
        {
            user = user ?? this.Context.User as SocketGuildUser;
            var embed = new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle($"INFORMATION | {user}")
                .WithThumbnailUrl(user?.GetAvatarUrl());
            var roles = new List<IRole>();
            var roleIds = ((IGuildUser)user)?.RoleIds;
            if (roleIds != null)
            {
                roles.AddRange(roleIds.Select(r => this.Context.Guild.GetRole(r)));
            }

            if (user != null)
            {
                embed.AddField("Nickname", user.Nickname ?? user.Username, true);
                embed.AddField("ID", user.Id, true);
                embed.AddField("Muted?", user.IsMuted ? "Yes" : "No", true);
                embed.AddField("Is Bot?", user.IsBot ? "Yes" : "No", true);
                embed.AddField("Created", user.CreatedAt.ToPragueTimeString(), true);
                embed.AddField("Joined guild", user.JoinedAt.ToPragueTimeString(), true);
                embed.AddField("Status", user.Status, true);
                embed.AddField("Playing", user.Activity?.ToString() ?? "*NOTHING*", true);
                embed.AddField("Permissions", string.Join(", ", user.GuildPermissions.ToList()), true);
                embed.AddField("Roles", string.Join(", ", roles.OrderByDescending(x => x.Position)), true);
            }

            return this.ReplyAsync(string.Empty, embed: embed.Build());
        }

        [Command("guildinfo"), Alias("gi"), Remarks("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.LightOrange)
                .WithThumbnailUrl(this.Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256")
                .WithTitle($"INFORMATION | {this.Context.Guild.Name}");
            var guild = this.Context.Guild as IGuild;
            embed.AddField("ID", this.Context.Guild.Id, true);
            embed.AddField("Owner", await guild.GetOwnerAsync(), true);
            embed.AddField("Default Channel", await guild.GetDefaultChannelAsync(), true);
            embed.AddField("Voice Region", this.Context.Guild.VoiceRegionId, true);
            embed.AddField("Created At", this.Context.Guild.CreatedAt.ToPragueTimeString(), true);
            embed.AddField("Roles", $"{this.Context.Guild.Roles.Count }\n{string.Join(", ", this.Context.Guild.Roles.OrderByDescending(x => x.Position))}", true);
            embed.AddField("Users", (await guild.GetUsersAsync()).Count(x => x.IsBot == false), true);
            embed.AddField("Bots", (await guild.GetUsersAsync()).Count(x => x.IsBot), true);
            embed.AddField("Text Channels", this.Context.Guild.TextChannels.Count, true);
            embed.AddField("Voice Channels", this.Context.Guild.VoiceChannels.Count, true);
            await this.ReplyAsync(string.Empty, false, embed.Build());
        }

        [Command("roleinfo"), Alias("ri"), Remarks("Displays information about a role.")]
        public Task RoleInfoAsync(IRole role)
        {
            var embed = new EmbedBuilder()
                .WithColor(role.Color)
                .WithTitle($"INFORMATION | {role.Name}");
            embed.AddField("ID", role.Id, true);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Creation Date", role.CreatedAt.ToPragueTimeString(), true);
            embed.AddField("Is Hoisted?", role.IsHoisted ? "Yes" : "No", true);
            embed.AddField("Is Mentionable?", role.IsMentionable ? "Yes" : "No", true);
            embed.AddField("Is Managed?", role.IsManaged ? "Yes" : "No", true);
            embed.AddField("Permissions", string.Join(", ", role.Permissions.ToList()), true);
            return this.ReplyAsync(string.Empty, embed: embed.Build());
        }

        [Command("avatar"), Summary("Shows users avatar in high resolution.")]
        public Task UserAvatarAsync(SocketUser user = null)
        {
            user = user ?? this.Context.User;
            var avatar = user.GetAvatarUrl(size: 2048) ?? "This user has no avatar";
            return this.ReplyAsync(avatar);
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}
// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using LennyBOT.Config;

    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService service;

        public HelpModule(CommandService service)
        {
            // Create a constructor for the commandservice dependency
            this.service = service;
        }

        /*
                [Command("help")]
                [Remarks("Print available commands")]
                [MinPermissions(AccessLevel.User)]
                public async Task HelpAsync()
                {
                    string prefix = Configuration.Load().Prefix.ToString();
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(114, 137, 218),
                        Description = "These are the commands you can use:"
                    };

                    foreach (var module in _service.Modules)
                    {
                        string description = null;
                        foreach (var cmd in module.Commands)
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context);
                            if (result.IsSuccess)
                                description += $"{prefix}{cmd.Aliases.First()}\n";
                        }

                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            builder.AddField(x =>
                            {
                                x.Name = module.Name;
                                x.Value = description;
                                x.IsInline = false;
                            });
                        }
                    }
                    await ReplyAsync("", false, builder.Build());
                }
                */

        [Command("info")]
        [Alias("about")]
        [Remarks("Get info about bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task InfoCmdAsync()
        {
            var app = await this.Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            await this.ReplyAsync(
                $"{Format.Bold("Info")}\n" + $"- Author: {app.Owner} ({app.Owner.Id})\n"
                + $"- Library: Discord.Net ({DiscordConfig.Version})\n"
                + $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} "
                + $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n"
                + $"- Uptime: {GetUptime()}\n\n" + $"{Format.Bold("Stats")}\n" + $"- Heap Size: {GetHeapSize()}MiB\n"
                + $"- Guilds: {this.Context.Client.Guilds.Count}\n"
                + $"- Channels: {this.Context.Client.Guilds.Sum(g => g.Channels.Count)}\n"
                + $"- Users: {this.Context.Client.Guilds.Sum(g => g.Users.Count)}\n")
                .ConfigureAwait(false);
        }

        [Command("debug")]
        [Remarks("Get ids of guild, channel, user")]
        [MinPermissions(AccessLevel.User)]
        public Task DebugCmdAsync()
        {
            var serverId = (this.Context.Channel as IGuildChannel)?.GuildId.ToString() ?? "n/a";
            var response =
                $"```Guild ID:   {serverId}\nChannel ID: {this.Context.Channel.Id}\nUser ID:    {this.Context.User.Id}```";
            return this.ReplyAsync(response);
        }

        [Command("help")]
        [Remarks("Get help about command")]
        [MinPermissions(AccessLevel.User)]
        public async Task HelpCmdAsync(string command = null)
        {
            command = command ?? "dasdkajdnjkasdkads@&$²`124578";
            if (command == "dasdkajdnjkasdkads@&$²`124578")
            {
                var prefix = Configuration.Load().Prefix.ToString();
                var builder = new EmbedBuilder
                                  {
                                      Color = new Color(114, 137, 218),
                                      Description = "These are the commands you can use:"
                                  };

                foreach (var module in this.service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(this.Context).ConfigureAwait(false);
                        if (result.IsSuccess)
                        {
                            description += prefix + string.Join($", {prefix}", cmd.Aliases) + "\n";
                        }
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

                await this.ReplyAsync(string.Empty, false, builder.Build()).ConfigureAwait(false);
            }
            else
            {
                var result = this.service.Search(this.Context, command);

                if (!result.IsSuccess)
                {
                    await this.ReplyAsync($"Sorry, I couldn't find a command like **{command}**.").ConfigureAwait(false);
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

                    builder.AddField(
                        x =>
                            {
                                x.Name = string.Join(", ", cmd.Aliases);
                                x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n"
                                          + $"Description: {cmd.Remarks}\n";
                                x.IsInline = false;
                            });
                }

                await this.ReplyAsync(string.Empty, false, builder.Build()).ConfigureAwait(false);
            }
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}

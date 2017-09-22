// ReSharper disable StyleCop.SA1600
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using LennyBOT.Config;

namespace LennyBOT.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            // Create a constructor for the commandservice dependency
            this._service = service;
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
        private static string GetUptime() =>
            (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        [Command("info")]
        [Alias("about")]
        [Remarks("Get info about bot")]
        [MinPermissions(AccessLevel.User)]
        public async Task InfoCmd()
        {
            var app = await this.Context.Client.GetApplicationInfoAsync();

            await this.ReplyAsync(
                // $"Lenny is bot.\n\n" +
                $"{Format.Bold("Info")}\n" + $"- Author: {app.Owner} ({app.Owner.Id})\n"
                + $"- Library: Discord.Net ({DiscordConfig.Version})\n"
                + $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} "
                + $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n"
                + $"- Uptime: {GetUptime()}\n\n" + $"{Format.Bold("Stats")}\n" + $"- Heap Size: {GetHeapSize()}MiB\n"
                + $"- Guilds: {this.Context.Client.Guilds.Count}\n"
                + $"- Channels: {this.Context.Client.Guilds.Sum(g => g.Channels.Count)}\n"
                + $"- Users: {this.Context.Client.Guilds.Sum(g => g.Users.Count)}\n");
        }

        [Command("debug")]
        [Remarks("Get ids of guild, channel, user")]
        [MinPermissions(AccessLevel.User)]
        public async Task DebugCmd()
        {
            var serverId = (this.Context.Channel as IGuildChannel)?.GuildId.ToString() ?? "n/a";
            var response =
                $"```Guild ID:   {serverId}\nChannel ID: {this.Context.Channel.Id}\nUser ID:    {this.Context.User.Id}```";
            await this.ReplyAsync(response);
        }

        [Command("help")]
        [Remarks("Get help about command")]
        [MinPermissions(AccessLevel.User)]
        public async Task HelpCmd(string command = null)
        {
            command = command ?? "dasdkajdnjkasdkads@&$²`124578";
            if (command == "dasdkajdnjkasdkads@&$²`124578")
            {
                var prefix = Configuration.Load().Prefix.ToString();
                var builder = new EmbedBuilder()
                                  {
                                      Color = new Color(114, 137, 218),
                                      Description = "These are the commands you can use:"
                                  };

                foreach (var module in this._service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(this.Context);
                        if (result.IsSuccess)

                            // description += $"{prefix}{cmd.Aliases.First()}\n";
                            description += prefix + string.Join($", {prefix}", cmd.Aliases) + "\n";
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
                var result = this._service.Search(this.Context, command);

                if (!result.IsSuccess)
                {
                    await this.ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                var prefix = Configuration.Load().Prefix.ToString();
                var builder = new EmbedBuilder()
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
                                          + $"Description: {cmd.Remarks}\n" /*+
                              $"Can use: "/*{ cmd. Parameters.Select(p => p.Name) }/ +"and higher"*/;
                                x.IsInline = false;
                            });
                }

                await this.ReplyAsync(string.Empty, false, builder.Build());
            }
        }
    }
}

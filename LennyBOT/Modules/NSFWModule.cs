// ReSharper disable StyleCop.SA1600
/*/ ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Models;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class NsfwModule : LennyBase
    {
        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!"), Alias("Tits")]
        public async Task BoobsAsync()
        {
            using (var client = new HttpClient())
            {
                var token = JArray.Parse(
                    await client.GetStringAsync($"http://api.oboobs.ru/boobs/{new Random().Next(0, 10229)}")
                        .ConfigureAwait(false))[0];
                await this.ReplyAsync($"http://media.oboobs.ru/{token["preview"].ToString()}").ConfigureAwait(false);
            }
        }

        [Command("Ass"), Summary("I can't believe you need help with this command."), Alias("Butt")]
        public async Task BumsAsync()
        {
            using (var client = new HttpClient())
            {
                var token = JArray.Parse(
                    await client.GetStringAsync($"http://api.obutts.ru/butts/{new Random().Next(0, 4963)}").ConfigureAwait(false))[0];
                await this.ReplyAsync($"http://media.obutts.ru/{token["preview"].ToString()}").ConfigureAwait(false);
            }
        }

        [Command("Porn"), Remarks("Uses Porn.com API to fetch videos.")]
        public async Task PornAsync([Remainder] string search)
        {
            using (var client = new HttpClient())
            {
                var response = await client
                                   .GetAsync(
                                       $"http://api.porn.com/videos/find.json?search={Uri.EscapeDataString(search)}")
                                   .ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    await this.ReplyAsync(response.ReasonPhrase).ConfigureAwait(false);
                    return;
                }

                var convertJson = JsonConvert.DeserializeObject<Porn>(
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (!convertJson.IsSuccess)
                {
                    await this.ReplyAsync("No results found!").ConfigureAwait(false);
                    return;
                }

                var getvid = convertJson.VideoModel[new Random().Next(0, 20)];
                var time = TimeSpan.FromSeconds(getvid.Duration);
                var embed = new EmbedBuilder().WithColor(Color.LightGrey).WithThumbnailUrl(getvid.VideoThumb)
                    .WithTitle(getvid.VideoTitle).WithDescription(getvid.VideoUrl)
                    .WithFooter($"Total Results: {convertJson.Count}");
                embed.AddField("Video Length", time, true);
                embed.AddField("Total Views", getvid.Views, true);
                await this.ReplyAsync(string.Empty, embed: embed.Build()).ConfigureAwait(false);
            }
        }

        /*[Command("pornhub"), Alias("ph")]
        [Remarks("Search for PornHub video")]
        [MinPermissions(AccessLevel.User)]
        public Task PornhubCmdAsync([Remainder] string keywords)
        {
            var query = keywords.Replace(" ", "+");
            using (var client = new WebClient())
            {
                var html = client.DownloadString("https://www.pornhub.com/video/search?search=" + query);
                return this.ReplyAsync("");
            }
        }*
    }
}
////*/
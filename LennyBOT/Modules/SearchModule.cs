// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using CsfdAPI;

    using Discord;
    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Models;
    using LennyBOT.Services;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Name("Search")]
    public class SearchModule : LennyBase
    {
        [Command("define", RunMode = RunMode.Async)]
        [Alias("def", "urban")]
        [Remarks("Look up a definition in UrbanDictionary")]
        [MinPermissions(AccessLevel.User)]
        public async Task DefineCmdAsync([Remainder] string query)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(
                                 $"http://api.urbandictionary.com/v0/define?term={query.Replace(' ', '+')}");
                if (!result.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Couldn't communicate with UrbanDictionary API.");
                    return;
                }

                var data = JToken.Parse(await result.Content.ReadAsStringAsync()).ToObject<Urban>();
                if (!data.List.Any())
                {
                    await this.ReplyAsync($"Couldn't find anything related to *{query}*.");
                    return;
                }

                var termInfo = data.List.FirstOrDefault(); // .List[new Random().Next(0, data.List.Count)];
                var embed = new EmbedBuilder()
                    .WithUrl("https://www.urbandictionary.com/define.php?term=" + query.Replace(' ', '+'))
                    .WithColor(new Color(255, 84, 33))
                    .WithCurrentTimestamp()
                    .WithAuthor("Urban Dictionary", "https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png", "https://urbandictionary.com")
                    .WithFooter($"Related Terms: {string.Join(", ", data.Tags)}" ?? "No related terms.")
                    .AddField($"Definition of {termInfo?.Word ?? query}", termInfo?.Definition ?? string.Empty)
                    .AddField("Example", termInfo?.Example ?? string.Empty);
                await this.ReplyAsync(string.Empty, embed: embed.Build());
            }
        }

        [Command("youtube", RunMode = RunMode.Async)]
        [Alias("yt")]
        [Remarks("Look up a video on YouTube")]
        [MinPermissions(AccessLevel.User)]
        public Task YouTubeCmdAsync([Remainder] string query)
        {
            using (var client = new WebClient())
            {
                var html = client.DownloadString(
                    "https://www.youtube.com/results?search_query=" + Regex.Replace(query, @"\s+", "+"));
                return this.ReplyAsync(
                    "https://www.youtube.com/watch?" + Regex.Split(Regex.Split(html, @"\/watch\?")[1], "\"")[0]);
            }
        }

        [Command("lmgtfy"), Remarks("Googles something for that special person who is crippled")]
        [MinPermissions(AccessLevel.User)]
        public Task LmgtfyAsync([Remainder] string search = "How to use Lmgtfy")
            => this.ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search)}>");

        [Command("wiki"), Remarks("Searches Wikipedia for your terms")]
        public async Task WikiAsync([Remainder]string search)
        {
            using (var client = new HttpClient())
            {
                var getResult = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={ search}");

                if (!getResult.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Couldn't communicate with Wikipedia API.");
                    return;
                }

                var getContent = await getResult.Content.ReadAsStringAsync();
                dynamic responseObject = JsonConvert.DeserializeObject(getContent);
                string title = responseObject[1][0];
                string firstParagraph = responseObject[2][0];
                string url = responseObject[3][0];

                var builder = new StringBuilder();
                foreach (var urls in responseObject[3])
                {
                    builder.AppendLine($"<{urls.ToString()}>");
                }

                if (string.IsNullOrWhiteSpace(firstParagraph))
                {
                    await this.ReplyAsync("No results found.");
                }
                else
                {
                    await this.ReplyAsync($"{title}: {url}\n**See also:**\n{builder}");
                }
            }
        }

        [Command("csfd", RunMode = RunMode.Async)]
        [Remarks("Look up a movie on ČSFD")]
        [MinPermissions(AccessLevel.User)]
        public Task CsfdCmdAsync([Remainder] string query)
        {
            var client = new CsfdApi();
            var search = client.SearchMovie(query);

            var embed = new EmbedBuilder().WithDescription($"{search.Plot}").WithUrl($"{search.Url}")
                .WithColor(new Color(255, 0, 0)).WithCurrentTimestamp()
                .WithThumbnailUrl("https://img.csfd.cz/documents/marketing/logos/logo-white-red/logo-white-red.svg")
                .WithImageUrl($"{search.PosterUrl}")
                .WithTitle($"{search.Title} ({search.Year}) {search.Rating}%")
                .WithUrl($"{search.Url}").AddField("Žánry", $"{string.Join(", ", search.Genres)}")
                .Build();
            return this.ReplyAsync(string.Empty, false, embed);
        }

        [Command("imdb", RunMode = RunMode.Async)]
        [Remarks("Look up a movie on IMDb")]
        [MinPermissions(AccessLevel.User)]
        public async Task ImdbCmdAsync([Remainder] string query)
        {
            var reply = await SearchService.SearchImdbAsync(query);
            await this.ReplyAsync(reply);
        }

        [Command("steam game", RunMode = RunMode.Async)]
        [Alias("steam g")]
        [Remarks("Steam game info")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamGameCmdAsync([Remainder] string game = null)
        {
            if (game == null)
            {
                await this.Context.Channel.SendMessageAsync("`Enter a game name`");
                return;
            }

            try
            {
                var searchQuery = game;
                var results = SteamStoreQuery.Query.Search(searchQuery);

                await this.Context.Channel.SendMessageAsync(results[0].StoreLink);
            }
            catch
            {
                await this.Context.Channel.SendMessageAsync("`Could not find game`");
            }
        }

        [Command("steam user", RunMode = RunMode.Async)]
        [Alias("steam u")]
        [Remarks("Steam user info")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamUserCmdAsync([Remainder] string userId = null)
        {
            try
            {
                var embed = await SearchService.SearchSteamUserAsync(userId);
                await this.ReplyAsync(string.Empty, embed: embed);
            }
            catch
            {
                await this.ReplyAsync(
                        "**Error** \nAre you sure you entered a correct steamID?\nYou can get steamID here: <http://steamidfinder.com/>. Use *steamID64*")
                    ;
            }
        }

        [Command("steam inventory", RunMode = RunMode.Async)]
        [Alias("steam inv")]
        [Remarks("Get inventory value of player from http://csgobackpack.net")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamInventoryCmdAsync(string steamId64)
        {
            // var user = this.Context.Message.Author as SocketGuildUser;
            using (var webClient = new HttpClient())
            {
                var result = await webClient.GetAsync($"http://csgobackpack.net/api/GetInventoryValue/?id={steamId64}")
                                 ;
                if (!result.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Couldn't communicate with CSGOBackPack API.");
                    return;
                }

                var jsonContent = await result.Content.ReadAsStringAsync();
                var inventoryData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var messageContent = inventoryData.Success
                                         ? $"**Player:** {steamId64}\n**Inventory value:** {inventoryData.Value} {inventoryData.Currency}\n**Items:** {inventoryData.Items}"
                                         : "**Error** \nAre you sure you entered a correct steamID?\nYou can get steamID here: <http://steamidfinder.com/>. Use *steamID64*";
                await this.ReplyAsync(messageContent);
            }
        }

        [Command("osu", RunMode = RunMode.Async)]
        [Remarks("Get osu! user info")]
        [MinPermissions(AccessLevel.User)]
        public async Task OsuCmdAsync(string user = null)
        {
            var osuApiKey = Config.Configuration.Load().OsuApiKey;
            try
            {
                dynamic osuData;
                using (var webClient = new HttpClient())
                {
                    var result = await webClient.GetAsync($"https://osu.ppy.sh/api/get_user?k={osuApiKey}&u={user}")
                                     ;
                    if (!result.IsSuccessStatusCode)
                    {
                        await this.ReplyAsync("Couldn't communicate with Osu! API.");
                        return;
                    }

                    var jsonContent = await result.Content.ReadAsStringAsync();
                    var osuDatas = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);
                    osuData = osuDatas.FirstOrDefault();
                }

                var acc = double.Parse(osuData?.Accuracy.Replace('.', ','));
                var embed = new EmbedBuilder()
                    .WithTitle($"{osuData?.Username} | (Level: {osuData?.Level.Split('.').First()})")
                    .WithThumbnailUrl("https://s.ppy.sh/images/head-logo.png")
                    .WithUrl($"https://osu.ppy.sh/u/{osuData?.Username}").WithColor(new Color(255, 105, 180))
                    .WithCurrentTimestamp()
                    .AddField(
                        "Stats",
                        $"```json\nPlayed songs: {osuData?.Playcount}\n    Accuracy: {string.Format("{0:F2}", acc).Replace(',', '.')}```",
                        true)
                    .AddField(
                        "Score",
                        $"```json\n Total: {osuData?.Total_score}\nRanked: {osuData?.Ranked_score}```",
                        true).AddField(
                        "Performance points",
                        $"```json\nPerformance points: {osuData?.Pp_raw}\nWorldwide rank: {osuData?.Pp_rank}\nCountry rank: {osuData?.Pp_country_rank} ({osuData?.Country})```",
                        true).AddField(
                        "Rank",
                        $"```json\nSS: {osuData?.Count_rank_ss}\n S: {osuData?.Count_rank_s}\n A: {osuData?.Count_rank_a}```",
                        true).Build();
                await this.ReplyAsync(string.Empty, false, embed);
            }
            catch (Exception)
            {
                await this.ReplyAsync("`Cannot find osu user`");
            }
        }

        [Command("Trump"), Remarks("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync()
        {
            using (var client = new HttpClient())
            {
                var get = await client.GetAsync("https://api.tronalddump.io/random/quote");
                if (!get.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Using TrumpDump API was the worse trade deal, maybe ever.");
                    return;
                }

                var jsonObj = JObject.Parse(await get.Content.ReadAsStringAsync());
                var embedB = new EmbedBuilder()
                    .WithAuthor("Donald Trump", string.Empty, "https://www.tronalddump.io")
                    .WithTitle(string.Empty)
                    .WithThumbnailUrl("https://i.redd.it/0x4bejtshuhx.png")
                    .WithColor(Color.DarkBlue)
                    .WithDescription(jsonObj["value"].ToString());

                // ReSharper disable once InlineOutVariableDeclaration
                DateTimeOffset timestamp;
                if (DateTimeOffset.TryParse(jsonObj["appeared_at"].ToString(), out timestamp))
                {
                    timestamp = timestamp.AddHours(-timestamp.Offset.Hours);
                    embedB.Timestamp = timestamp;
                }

                await this.ReplyAsync(string.Empty, false, embedB.Build());
            }
        }

        [Command("yomama"), Remarks("Gets a random yomama Joke"), Alias("yomomma", "yomom", "your mom")]
        public async Task YomamaAsync()
        {
            using (var client = new HttpClient())
            {
                var get = await client.GetAsync("http://api.yomomma.info/");
                if (!get.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Yo mama so fat she crashed Yomomma's API.");
                    return;
                }

                var jsonObj = JObject.Parse(await get.Content.ReadAsStringAsync());
                await this.ReplyAsync(jsonObj["joke"].ToString());
            }
        }

        [Command("torrent", RunMode = RunMode.Async), Remarks("")]
        public async Task TorrentAsync(params string[] keywords)
        {
            try
            {
                var embed = await SearchService.SearchTorrentsAsync(keywords);

                await this.ReplyAsync(string.Empty, false, embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await this.ReplyAsync("Couldn't reach the server. Try again later");
            }
        }
    }
}

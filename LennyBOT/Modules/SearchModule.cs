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

    using AngleSharp;

    using Cookie.Steam;

    using CsfdAPI;

    using Discord;
    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Models;
    using LennyBOT.Services;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using SteamStoreQuery;


    [Name("Search")]
    public class SearchModule : LennyBase
    {
        [Command("define", RunMode = RunMode.Async)]
        [Alias("def", "urban")]
        [Remarks("Look up a definition in UrbanDictionary")]
        [MinPermissions(AccessLevel.User)]
        public async Task DefineCmdAsync([Remainder] string query)
        {
            using (var c = new HttpClient())
            {
                var client = await c.GetAsync(
                                 $"http://api.urbandictionary.com/v0/define?term={query.Replace(' ', '+')}").ConfigureAwait(false);
                if (!client.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Couldn't communicate with Urban's API.").ConfigureAwait(false);
                    return;
                }

                var data = JToken.Parse(await client.Content.ReadAsStringAsync().ConfigureAwait(false)).ToObject<Urban>();
                if (!data.List.Any())
                {
                    await this.ReplyAsync($"Couldn't find anything related to *{query}*.").ConfigureAwait(false);
                    return;
                }

                var termInfo = data.List[new Random().Next(0, data.List.Count)];
                var embed = new EmbedBuilder()
                    .WithUrl("https://www.urbandictionary.com/define.php?term=" + query.Replace(' ', '+'))
                    .WithColor(new Color(255, 84, 33))
                    .WithCurrentTimestamp()
                    .WithAuthor("Urban Dictionary", "https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png", "https://urbandictionary.com")
                    .WithFooter($"Related Terms: {string.Join(", ", data.Tags)}" ?? "No related terms.")
                    .AddField($"Definition of {termInfo.Word}", termInfo.Definition)
                    .AddField("Example", termInfo.Example);
                await this.ReplyAsync(string.Empty, embed: embed.Build()).ConfigureAwait(false);
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
            => this.ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search) }>");

        [Command("wiki"), Remarks("Searches wikipedia for your terms")]
        public async Task WikiAsync([Remainder]string search)
        {
            using (var client = new HttpClient())
            {
                var getResult = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search={search}").ConfigureAwait(false);
                var getContent = await getResult.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                    await this.ReplyAsync("No results found.").ConfigureAwait(false);
                }
                else
                {
                    await this.ReplyAsync($"{title}: {url}\n**See also:**\n{builder}").ConfigureAwait(false);
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
                .WithImageUrl($"{search.PosterUrl}").WithTitle($"{search.Title} ({search.Year}) {search.Rating}%")
                .WithUrl($"{search.Url}").AddField("Žánry", $"{string.Join(", ", search.Genres)}").Build();
            return this.ReplyAsync(string.Empty, false, embed);
        }

        [Command("imdb", RunMode = RunMode.Async)]
        [Remarks("Look up a movie on IMDb")]
        [MinPermissions(AccessLevel.User)]
        public async Task ImdbCmdAsync([Remainder] string query)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var address = "http://www.imdb.com/find?s=tt&q=" + query.Replace(" ", "+");
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address).ConfigureAwait(false);
            const string CellSelector = "td.result_text";
            var cells = document.QuerySelectorAll(CellSelector).ToList();
            var reply = string.Empty;
            for (var i = 0; i < 4; i++)
            {
                var title = cells[i].TextContent.Trim(' ');
                var url = "https://www.imdb.com" + cells[i].InnerHtml.Split('"')[1].Split('?')[0];

                // -------- rating
                var rating = string.Empty;
                if (i == 0)
                {
                    var document2 = await context.OpenAsync(url).ConfigureAwait(false);
                    var sourceText = document2.Source.Text;
                    const string Searchtext = "<span itemprop=\"ratingValue\">";
                    rating = SearchService.Extract(sourceText, Searchtext, "</span>");

                    // ReSharper disable once StyleCop.SA1126
                    if (!double.TryParse(rating.Replace('.', ','), out _))
                    {
                        rating = "N/A";
                    }
                    else
                    {
                        rating += "/10";
                    }
                }

                // -------- */
                if (i == 0)
                {
                    reply += $"{title} *Rating: {rating}*\n{url}\n\n**Other results:**\n";
                }
                else
                {
                    reply += $"{title}\n<{url}>\n";
                }
            }

            await this.ReplyAsync(reply).ConfigureAwait(false);
        }

        [Command("steam game", RunMode = RunMode.Async)]
        [Alias("steam g")]
        [Remarks("Steam game info")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamGameCmdAsync([Remainder] string game = null)
        {
            if (game == null)
            {
                await this.Context.Channel.SendMessageAsync("`Enter a game name`").ConfigureAwait(false);
                return;
            }

            try
            {
                var searchQuery = game;
                var results = Query.Search(searchQuery);
                /*var cost = string.Empty;
                if (results[0].SaleType.ToString() == "FreeToPlay")
                {
                    cost = "Free!";
                }
                else
                {
                    cost = $"${results[0].PriceUSD}";
                }*/

                await this.Context.Channel.SendMessageAsync(results[0].StoreLink).ConfigureAwait(false);
            }
            catch
            {
                await this.Context.Channel.SendMessageAsync("`Could not find game`").ConfigureAwait(false);
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
                var steamClient = new SteamClient(Config.Configuration.Load().SteamApiKey);
                var userInfo = await steamClient.GetUsersInfoAsync(new List<string> { userId }).ConfigureAwait(false);
                var userGames = await steamClient.OwnedGamesAsync(userId).ConfigureAwait(false);
                var userRecent = await steamClient.RecentGamesAsync(userId).ConfigureAwait(false);

                var info = userInfo.PlayersInfo.Players.FirstOrDefault();

                string state;
                switch (info?.ProfileState)
                {
                    case 0:
                        state = "Offline";
                        break;
                    case 1:
                        state = "Online";
                        break;
                    case 2:
                        state = "Busy";
                        break;
                    case 3:
                        state = "Away";
                        break;
                    case 4:
                        state = "Snooze";
                        break;
                    case 5:
                        state = "Looking to trade";
                        break;
                    case null:
                        state = null;
                        break;
                    default:
                        state = "Looking to play";
                        break;
                }

                var embed = new EmbedBuilder()
                    .WithColor(Color.DarkBlue)
                    .WithThumbnailUrl(info?.AvatarFullUrl)
                    .WithTitle(info?.RealName ?? info?.Name)
                    .WithUrl(info?.ProfileLink)
                    .WithCurrentTimestamp();
                embed.AddField("Display Name", $"{info?.Name}", true);
                embed.AddField("Status", state, true);
                embed.AddField("Profile Created", SearchService.DateFromSeconds(info?.TimeCreated), true);
                embed.AddField("Last Online", SearchService.DateFromSeconds(info?.LastLogOff), true);
                embed.AddField("Country", $"{info?.Country ?? "No Country"}", true);
                ////embed.AddField("Primary Clan ID", info.PrimaryClanId, true);
                embed.AddField("Owned Games", userGames.OwnedGames.GamesCount, true);
                embed.AddField("Recently Played Games", $"{userRecent.RecentGames.TotalCount}\n{string.Join(", ", userRecent.RecentGames.GamesList.Select(x => x.Name))}", true);

                await this.ReplyAsync(string.Empty, embed: embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                await this.ReplyAsync(
                        "**Error** \nAre you sure you entered a correct steamID?\nYou can get steamID here: <http://steamidfinder.com/>. Use *steamID64*")
                    .ConfigureAwait(false);
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
                                 .ConfigureAwait(false);
                var jsonContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var inventoryData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var messageContent = inventoryData.Success
                                         ? $"**Player:** {steamId64}\n**Inventory value:** {inventoryData.Value} {inventoryData.Currency}\n**Items:** {inventoryData.Items}"
                                         : "**Error** \nAre you sure you entered a correct steamID?\nYou can get steamID here: <http://steamidfinder.com/>. Use *steamID64*";
                await this.ReplyAsync(messageContent).ConfigureAwait(false);
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
                                     .ConfigureAwait(false);
                    var jsonContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                await this.ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await this.ReplyAsync("`Cannot find osu user`").ConfigureAwait(false);
            }
        }

        [Command("Trump"), Remarks("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync()
        {
            using (var client = new HttpClient())
            {
                var get = await client.GetAsync("https://api.tronalddump.io/random/quote").ConfigureAwait(false);
                if (!get.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Using TrumpDump API was the worse trade deal, maybe ever.").ConfigureAwait(false);
                    return;
                }

                var jsonObj = JObject.Parse(await get.Content.ReadAsStringAsync().ConfigureAwait(false));
                ////await this.ReplyAsync(jsonObj["value"].ToString()).ConfigureAwait(false);
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

                await this.ReplyAsync(string.Empty, false, embedB.Build()).ConfigureAwait(false);
            }
        }

        [Command("yomama"), Remarks("Gets a random yomama Joke"), Alias("yomomma", "yomom", "your mom")]
        public async Task YomamaAsync()
        {
            using (var client = new HttpClient())
            {
                var get = await client.GetAsync("http://api.yomomma.info/").ConfigureAwait(false);
                if (!get.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("Yo mama so fat she crashed Yomomma's API.").ConfigureAwait(false);
                    return;
                }

                var jsonObj = JObject.Parse(await get.Content.ReadAsStringAsync().ConfigureAwait(false));
                await this.ReplyAsync(jsonObj["joke"].ToString()).ConfigureAwait(false);
            }
        }
    }
}

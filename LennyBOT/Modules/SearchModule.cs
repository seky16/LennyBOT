using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOT.Config;
using LennyBOT.Services;
using AngleSharp;
using SteamStoreQuery;
using PortableSteam;
using System.IO;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using CsfdAPI;

namespace LennyBOT.Modules
{
    [Name("Search")]
    public class SearchModule : ModuleBase<SocketCommandContext>
    {
        private readonly SearchService _service;

        public SearchModule(SearchService service)
        {
            _service = service;
        }

        [Command("define", RunMode = RunMode.Async)]
        [Alias("def", "urban")]
        [Remarks("Look up a definition in UrbanDictionary")]
        [MinPermissions(AccessLevel.User)]
        public async Task DefineCmd([Remainder]string query)
        {
            var myDefiniton = await _service.UrbanClient.GetWordAsync(query);
            var defList = myDefiniton.List;

            var builder = new EmbedBuilder()
                .WithTitle($"{query}")
                .WithUrl("https://www.urbandictionary.com/define.php?term=" + query.Replace(' ', '+'))
                .WithColor(new Color(255, 84, 33))
                .WithTimestamp(DateTimeOffset.Now)
                .WithAuthor(author =>
                {
                    author
                        .WithName("Urban Dictionary")
                        .WithUrl("https://urbandictionary.com")
                        .WithIconUrl("https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png");
                });
            for (int i = 0; i < Math.Min(3, defList.Count()); i++)
            {
                string def = defList[i].Definition;
                if (def.Length > 250)
                {
                    def = def.Substring(0, 250) + "…";
                }
                string ex = defList[i].Example;
                if (ex.Length > 1010)
                {
                    ex = ex.Substring(0, 1010) + "…";
                }
                builder.AddField($"{i + 1}: {def}", $"Example: {ex}");
            }
            var embed = builder.Build();
            await ReplyAsync(String.Empty, false, embed);
        }

        [Command("youtube", RunMode = RunMode.Async)]
        [Alias("yt")]
        [Remarks("Look up a video on YouTube")]
        [MinPermissions(AccessLevel.User)]
        public async Task YouTubeCmd([Remainder]string query)
        {
            string html = new WebClient().DownloadString("https://www.youtube.com/results?search_query=" + Regex.Replace(query, @"\s+", "+"));
            await ReplyAsync("https://www.youtube.com/watch?" + Regex.Split(Regex.Split(html, @"\/watch\?")[1], "\"")[0]);
        }

        // todo: implement api
        [Command("csfd", RunMode = RunMode.Async)]
        [Remarks("Look up a movie on ČSFD")]
        [MinPermissions(AccessLevel.User)]
        public async Task CsfdCmd([Remainder]string query)
        {
            var client = new CsfdApi();
            var search = client.SearchMovie(query);

            var embed = new EmbedBuilder()
                .WithDescription($"{search.Plot}")
                .WithUrl($"{search.Url}")
                .WithColor(new Color(255, 0, 0))
                .WithCurrentTimestamp()
                .WithThumbnailUrl("https://img.csfd.cz/documents/marketing/logos/logo-white-red/logo-white-red.svg")
                .WithImageUrl($"{search.PosterUrl}")
                .WithTitle($"{search.Title} ({search.Year}) {search.Rating}%")
                .WithUrl($"{search.Url}")
                .AddField("Žánry", $"{String.Join(", ", search.Genres)}")
                .Build();
            await ReplyAsync(String.Empty, false, embed);
        }

        [Command("imdb", RunMode = RunMode.Async)]
        [Remarks("Look up a movie on IMDb")]
        [MinPermissions(AccessLevel.User)]
        public async Task ImdbCmd([Remainder]string query)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var address = "http://www.imdb.com/find?s=tt&q=" + query.Replace(" ", "+");
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);
            var cellSelector = "td.result_text";
            var cells = document.QuerySelectorAll(cellSelector).ToList();
            string reply = String.Empty;
            for (int i = 0; i < 4; i++)
            {
                var title = cells[i].TextContent.Trim(' ');
                var url = "https://www.imdb.com" + cells[i].InnerHtml.Split('"')[1].Split('?')[0];

                // -------- rating
                string rating = String.Empty;
                if (i == 0)
                {
                    var document2 = await context.OpenAsync(url);
                    var sourceText = document2.Source.Text;
                    string searchtext = "<span itemprop=\"ratingValue\">";
                    rating = _service.Extract(sourceText, searchtext, "</span>");
                    if (!Double.TryParse(rating.Replace('.', ','), out _)) { rating = "N/A"; }
                    else { rating += "/10"; };
                }
                // -------- */

                if (i == 0) { reply += $"{title} *Rating: {rating}*\n{url}\n\n**Other results:**\n"; }
                else { reply += $"{title}\n<{url}>\n"; }
            }
            await ReplyAsync(reply);
        }

        [Command("steam game", RunMode = RunMode.Async)]
        [Alias("steam g")]
        [Remarks("Steam game info")]
        [Summary("")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamGameCmd([Remainder] string Game = null)
        {
            if (Game == null)
            {
                await Context.Channel.SendMessageAsync("`Enter a game name`");
                return;
            }
            try
            {
                string searchQuery = Game;
                List<Listing> results = Query.Search(searchQuery);
                string cost = "";
                if (results[0].SaleType.ToString() == "FreeToPlay")
                {
                    cost = "Free!";
                }
                else
                {
                    cost = $"${results[0].PriceUSD}";
                }
                await Context.Channel.SendMessageAsync(results[0].StoreLink);

            }
            catch
            {
                await Context.Channel.SendMessageAsync("`Could not find game`");
            }

        }

        [Command("steam user", RunMode = RunMode.Async)]
        [Alias("steam u")]
        [Remarks("Steam user info")]
        [Summary("")]
        [MinPermissions(AccessLevel.User)]
        public async Task SteamUserCmd([Remainder] string user = null)
        {
            if (user == null)
            {
                await ReplyAsync("`You did not specify user`\nHow to get steam user? http://i.imgur.com/pM9dff5.png");
                return;
            }

            try
            {
                SteamIdentity SteamUser = null;
                SteamWebAPI.SetGlobalKey(Config.Configuration.Load().SteamAPIKey);
                SteamUser = SteamWebAPI.General().ISteamUser().ResolveVanityURL(user).GetResponse().Data.Identity;
                if (SteamUser == null)
                {
                    await ReplyAsync("`Could not find steam user`");
                    return;
                }
                //var Games = SteamWebAPI.General().IPlayerService().GetOwnedGames(SteamUser).GetResponse();
                var Badges = SteamWebAPI.General().IPlayerService().GetBadges(SteamUser).GetResponse();
                var LastPlayed = SteamWebAPI.General().IPlayerService().GetRecentlyPlayedGames(SteamUser).GetResponse();
                var Friends = SteamWebAPI.General().ISteamUser().GetFriendList(SteamUser, RelationshipType.Friend).GetResponse();

                //------------- avatarUrl + nickname
                var config = AngleSharp.Configuration.Default.WithDefaultLoader();
                var address = "https://steamcommunity.com/id/" + user;
                var document = await BrowsingContext.New(config).OpenAsync(address);
                var sourceText = document.Source.Text;
                var avatarUrl = _service.Extract(sourceText, "<div class=\"playerAvatarAutoSizeInner\"><img src=\"", "\"");
                var nickname = _service.Extract(sourceText, "<span class=\"actual_persona_name\">", "</span>");
                //-------------

                string Game1 = "";
                string Game2 = "";
                string Game3 = "";
                string Game4 = "";
                string Game5 = "";
                string GamePlay1 = "";
                string GamePlay2 = "";
                string GamePlay3 = "";
                string GamePlay4 = "";
                string GamePlay5 = "";
                if (LastPlayed.Data.TotalCount > 0)
                {
                    Game1 = LastPlayed.Data.Games[0].Name + "  ";
                    GamePlay1 = ((int)Math.Ceiling(LastPlayed.Data.Games[0].PlayTimeTotal.TotalHours)).ToString() + " hours";
                }
                if (LastPlayed.Data.TotalCount > 1)
                {
                    Game2 = LastPlayed.Data.Games[1].Name + "  ";
                    GamePlay2 = ((int)Math.Ceiling(LastPlayed.Data.Games[1].PlayTimeTotal.TotalHours)).ToString() + " hours";
                }
                if (LastPlayed.Data.TotalCount > 2)
                {
                    Game3 = LastPlayed.Data.Games[2].Name + "  ";
                    GamePlay3 = ((int)Math.Ceiling(LastPlayed.Data.Games[2].PlayTimeTotal.TotalHours)).ToString() + " hours";
                }
                if (LastPlayed.Data.TotalCount > 3)
                {
                    Game4 = LastPlayed.Data.Games[3].Name + "  ";
                    GamePlay4 = ((int)Math.Ceiling(LastPlayed.Data.Games[3].PlayTimeTotal.TotalHours)).ToString() + " hours";
                }
                if (LastPlayed.Data.TotalCount > 4)
                {
                    Game5 = LastPlayed.Data.Games[4].Name + "  ";
                    GamePlay5 = ((int)Math.Ceiling(LastPlayed.Data.Games[4].PlayTimeTotal.TotalHours)).ToString() + " hours";
                }

                var embed = new EmbedBuilder();
                {
                    embed.WithAuthor(author =>
                    {
                        author.WithName("Steam");
                        author.WithIconUrl("https://images.techhive.com/images/article/2016/11/steam_logo2-100691182-orig.jpg");
                        author.WithUrl("https://steamcommunity.com/");
                    });
                    embed.WithTitle($"{nickname} | Lvl {Badges.Data.PlayerLevel} | Xp {Badges.Data.PlayerXP}");
                    embed.WithUrl("https://steamcommunity.com/id/" + user);
                    embed.WithThumbnailUrl(avatarUrl);
                }
                embed.AddField(x =>
                {
                    x.Name = "Info"; x.Value = $"```json\nFriends: {Friends.Data.Friends.Count}\n" + /*$"<Games {Games.Data.GameCount}>" + Environment.NewLine +*/ $" Badges: {Badges.Data.Badges.Count}```";
                    x.IsInline = false;
                });
                embed.AddField(x =>
                {
                    x.Name = "Game";
                    x.Value = $"{Game1}\n{Game2}\n{Game3}\n{Game4}\n{Game5}";
                    x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = "Playtime";
                    x.Value = $"{GamePlay1}\n{GamePlay2}\n{GamePlay3}\n{GamePlay4}\n{GamePlay5}";
                    x.IsInline = true;
                });
                await ReplyAsync("", false, embed);
            }
            catch
            {
                await ReplyAsync("`Something went wrong!`");
            }
        }

        [Command("steam inventory", RunMode = RunMode.Async)]
        [Alias("steam inv")]
        [Remarks("Get inventory value of player")]
        [MinPermissions(AccessLevel.User)]
        public async Task Inventory(string steamID64)
        {
            var user = Context.Message.Author as SocketGuildUser;
            string messageContent;

            using (var webClient = new HttpClient())
            {
                var result = await webClient.GetAsync($"http://csgobackpack.net/api/GetInventoryValue/?id={steamID64}");
                string jsonContent = await result.Content.ReadAsStringAsync();
                var inventoryData = JsonConvert.DeserializeObject<Inventory>(jsonContent);
                messageContent = inventoryData.Success ?
                  $"**Player:** {steamID64}{Environment.NewLine}**Inventory value:** {inventoryData.Value} {inventoryData.Currency}{Environment.NewLine}**Items:** {inventoryData.Items}" :
                  $"**Error** {Environment.NewLine}Are you sure you entered a correct steamID?{Environment.NewLine}You can get your steamID here: <http://steamidfinder.com/> and use *steamID64*";
                await ReplyAsync(messageContent);
            }
        }

        [Command("osu", RunMode = RunMode.Async)]
        [Remarks("osu! user info")]
        [Summary("")]
        [MinPermissions(AccessLevel.User)]
        public async Task Osu(string user = null)
        {
            var osuAPIKey = Config.Configuration.Load().OsuAPIKey;
            try
            {
                OsuData osuData;
                using (var webClient = new HttpClient())
                {
                    var result = await webClient.GetAsync($"https://osu.ppy.sh/api/get_user?k={osuAPIKey}&u={user}");
                    string jsonContent = await result.Content.ReadAsStringAsync();
                    var osuDatas = JsonConvert.DeserializeObject<List<OsuData>>(jsonContent);
                    osuData = osuDatas.FirstOrDefault();
                }
                var acc = Double.Parse(osuData.Accuracy.Replace('.', ','));
                var embed = new EmbedBuilder()
                    .WithTitle($"{osuData.Username} | (Level: {osuData.Level.Split('.').First()})")
                    .WithThumbnailUrl("https://s.ppy.sh/images/head-logo.png")
                    .WithUrl($"https://osu.ppy.sh/u/{osuData.Username}")
                    .WithColor(new Color(255, 105, 180))
                    .WithCurrentTimestamp()
                    .AddInlineField("Stats", $"```json\nPlayed songs: {osuData.Playcount}\n    Accuracy: {String.Format("{0:F2}", acc).Replace(',', '.')}```")
                    .AddInlineField("Score", $"```json\n Total: {osuData.Total_score}\nRanked: {osuData.Ranked_score}```")
                    .AddInlineField("Performance points", $"```json\nPerformance points: {osuData.Pp_raw}\nWorldwide rank: {osuData.Pp_rank}\nCountry rank: {osuData.Pp_country_rank} ({osuData.Country})```")
                    .AddInlineField("Rank", $"```json\nSS: {osuData.Count_rank_ss}\n S: {osuData.Count_rank_s}\n A: {osuData.Count_rank_a}```")
                    .Build();
                await ReplyAsync("", false, embed);
            }
            catch (Exception)
            {
                await ReplyAsync($"`Cannot find osu user`");
            }
        }
    }
}

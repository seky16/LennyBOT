// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp;

    using CsfdAPI;

    using Discord;
    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Services;

    using Newtonsoft.Json;

    using PortableSteam;

    using SteamStoreQuery;

    [Name("Search")]
    public class SearchModule : ModuleBase<SocketCommandContext>
    {
        private readonly SearchService service;

        public SearchModule(SearchService service)
        {
            this.service = service;
        }

        [Command("define", RunMode = RunMode.Async)]
        [Alias("def", "urban")]
        [Remarks("Look up a definition in UrbanDictionary")]
        [MinPermissions(AccessLevel.User)]
        public async Task DefineCmdAsync([Remainder] string query)
        {
            var myDefiniton = await this.service.UrbanClient.GetWordAsync(query).ConfigureAwait(false);
            var defList = myDefiniton.List;

            var builder = new EmbedBuilder().WithTitle($"{query}")
                .WithUrl("https://www.urbandictionary.com/define.php?term=" + query.Replace(' ', '+'))
                .WithColor(new Color(255, 84, 33)).WithTimestamp(DateTimeOffset.Now).WithAuthor(
                    author =>
                        {
                            author.WithName("Urban Dictionary").WithUrl("https://urbandictionary.com").WithIconUrl(
                                "https://d2gatte9o95jao.cloudfront.net/assets/apple-touch-icon-55f1ee4ebfd5444ef5f8d5ba836a2d41.png");
                        });
            for (var i = 0; i < Math.Min(3, defList.Count); i++)
            {
                var def = defList[i].Definition;
                if (def.Length > 250)
                {
                    def = def.Substring(0, 250) + "…";
                }

                var ex = defList[i].Example;
                if (ex.Length > 1010)
                {
                    ex = ex.Substring(0, 1010) + "…";
                }

                builder.AddField($"{i + 1}: {def}", $"Example: {ex}");
            }

            var embed = builder.Build();
            await this.ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
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
        public async Task SteamUserCmdAsync([Remainder] string user = null)
        {
            if (user == null)
            {
                await this.ReplyAsync(
                        "`You did not specify user`\nHow to get steam user? http://i.imgur.com/pM9dff5.png")
                    .ConfigureAwait(false);
                return;
            }

            try
            {
                SteamWebAPI.SetGlobalKey(Config.Configuration.Load().SteamApiKey);
                var steamUser = (await SteamWebAPI.General().ISteamUser().ResolveVanityURL(user).GetResponseAsync()
                                     .ConfigureAwait(false)).Data.Identity;
                if (steamUser == null)
                {
                    await this.ReplyAsync("`Could not find steam user`").ConfigureAwait(false);
                    return;
                }

                // var Games = SteamWebAPI.General().IPlayerService().GetOwnedGames(SteamUser).GetResponse();
                var badges = await SteamWebAPI.General().IPlayerService().GetBadges(steamUser).GetResponseAsync()
                                 .ConfigureAwait(false);
                var lastPlayed = await SteamWebAPI.General().IPlayerService().GetRecentlyPlayedGames(steamUser)
                                     .GetResponseAsync().ConfigureAwait(false);
                var friends = await SteamWebAPI.General().ISteamUser().GetFriendList(steamUser, RelationshipType.Friend)
                                  .GetResponseAsync().ConfigureAwait(false);

                // ------------- avatarUrl + nickname
                var config = AngleSharp.Configuration.Default.WithDefaultLoader();
                var address = "https://steamcommunity.com/id/" + user;
                var document = await BrowsingContext.New(config).OpenAsync(address).ConfigureAwait(false);
                var sourceText = document.Source.Text;
                var avatarUrl = SearchService.Extract(
                    sourceText,
                    "<div class=\"playerAvatarAutoSizeInner\"><img src=\"",
                    "\"");
                var nickname = SearchService.Extract(sourceText, "<span class=\"actual_persona_name\">", "</span>");

                // -------------
                var game1 = string.Empty;
                var game2 = string.Empty;
                var game3 = string.Empty;
                var game4 = string.Empty;
                var game5 = string.Empty;
                var gamePlay1 = string.Empty;
                var gamePlay2 = string.Empty;
                var gamePlay3 = string.Empty;
                var gamePlay4 = string.Empty;
                var gamePlay5 = string.Empty;
                if (lastPlayed.Data.TotalCount > 0)
                {
                    game1 = lastPlayed.Data.Games[0].Name + "  ";
                    gamePlay1 = (int)Math.Ceiling(lastPlayed.Data.Games[0].PlayTimeTotal.TotalHours) + " hours";
                }

                if (lastPlayed.Data.TotalCount > 1)
                {
                    game2 = lastPlayed.Data.Games[1].Name + "  ";
                    gamePlay2 = (int)Math.Ceiling(lastPlayed.Data.Games[1].PlayTimeTotal.TotalHours) + " hours";
                }

                if (lastPlayed.Data.TotalCount > 2)
                {
                    game3 = lastPlayed.Data.Games[2].Name + "  ";
                    gamePlay3 = (int)Math.Ceiling(lastPlayed.Data.Games[2].PlayTimeTotal.TotalHours) + " hours";
                }

                if (lastPlayed.Data.TotalCount > 3)
                {
                    game4 = lastPlayed.Data.Games[3].Name + "  ";
                    gamePlay4 = (int)Math.Ceiling(lastPlayed.Data.Games[3].PlayTimeTotal.TotalHours) + " hours";
                }

                if (lastPlayed.Data.TotalCount > 4)
                {
                    game5 = lastPlayed.Data.Games[4].Name + "  ";
                    gamePlay5 = (int)Math.Ceiling(lastPlayed.Data.Games[4].PlayTimeTotal.TotalHours) + " hours";
                }

                var builder = new EmbedBuilder().WithAuthor(
                        author =>
                            {
                                author.WithName("Steam");
                                author.WithIconUrl(
                                    "https://images.techhive.com/images/article/2016/11/steam_logo2-100691182-orig.jpg");
                                author.WithUrl("https://steamcommunity.com/");
                            }).WithTitle($"{nickname} | Lvl {badges.Data.PlayerLevel} | Xp {badges.Data.PlayerXP}")
                    .WithUrl("https://steamcommunity.com/id/" + user).WithThumbnailUrl(avatarUrl);

                builder.AddField(
                    x =>
                        {
                            x.Name = "Info";
                            x.Value = $"```json\nFriends: {friends.Data.Friends.Count}\n"
                                      + /*$"<Games {Games.Data.GameCount}>\n" +*/
                                      $" Badges: {badges.Data.Badges.Count}```";
                            x.IsInline = false;
                        });
                builder.AddField(
                    x =>
                        {
                            x.Name = "Game";
                            x.Value = $"{game1}\n{game2}\n{game3}\n{game4}\n{game5}";
                            x.IsInline = true;
                        });
                builder.AddField(
                    x =>
                        {
                            x.Name = "Playtime";
                            x.Value = $"{gamePlay1}\n{gamePlay2}\n{gamePlay3}\n{gamePlay4}\n{gamePlay5}";
                            x.IsInline = true;
                        });
                var embed = builder.Build();
                await this.ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
            catch
            {
                await this.ReplyAsync("`Something went wrong!`").ConfigureAwait(false);
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
        }*/
    }
}

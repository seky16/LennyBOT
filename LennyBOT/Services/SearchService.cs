// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using AngleSharp;
    using AngleSharp.Dom.Html;

    using Cookie.Steam;

    using Discord;

    using NodaTime;

    public class SearchService
    {
        public static string DateFromSeconds(long? seconds)
        {
            if (seconds == null)
            {
                return null;
            }

            var instant = Instant.FromUnixTimeSeconds((long)seconds);
            var zone = DateTimeZoneProviders.Tzdb["Europe/Prague"];
            var date = new ZonedDateTime(instant, zone);
            return $"{date.Day}.{date.Month}.{date.Year} {date.Hour}:{date.Minute:D2}:{date.Second:D2}";
        }

        public static async Task<string> SearchImdbAsync(string query)
        {
            var config = Configuration.Default.WithDefaultLoader();
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

            return reply;
        }

        public static async Task<Embed> SearchSteamUserAsync(string userId)
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
            embed.AddField("OwneRecentlyd Games", userGames.OwnedGames.GamesCount, true);
            embed.AddField(" Played Games", $"{userRecent.RecentGames.TotalCount}\n{string.Join(", ", userRecent.RecentGames.GamesList.Select(x => x.Name))}", true);
            return embed.Build();
        }

        public static async Task<Embed> SearchTorrentsAsync(string[] keywords)
        {
            var url = $"http://torrentz2.eu/search?f={string.Join('+', keywords)}";

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url).ConfigureAwait(false);
            var rows = document.QuerySelectorAll("dl").ToList();

            var builder = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithTitle($"Torrents for: {string.Join(' ', keywords)}")
                .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/9/9d/Pirates.svg/2000px-Pirates.svg.png");

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    var row = rows[i];
                    var name = row.Children.FirstOrDefault()?.Children.FirstOrDefault()?.InnerHtml;
                    var hash = (row.Children.FirstOrDefault()?.Children.FirstOrDefault() as IHtmlAnchorElement)
                        ?.PathName.Substring(1).ToUpper();
                    ////var magnet = $"magnet:?xt=urn:btih:{hash}&dn={name}";
                    var torrent = $"http://itorrents.org/torrent/{hash}.torrent";
                    var age = row.Children.LastOrDefault()?.Children[1].InnerHtml;
                    var size = row.Children.LastOrDefault()?.Children[2].InnerHtml;
                    var seeders = row.Children.LastOrDefault()?.Children[3].InnerHtml;
                    var leechers = row.Children.LastOrDefault()?.Children[4].InnerHtml;

                    builder.AddField(
                        name,
                        $"[.torrent]({torrent}) | {age} ago | {size} | S: {seeders} | L: {leechers}\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception at {i}-th cycle: {e}");
                }
            }

            return builder.Build();
        }

        private static string Extract(string input, string start, string end)
        {
            int startNum, endNum;
            startNum = input.IndexOf(start, StringComparison.Ordinal) + start.Length;
            input = input.Remove(0, startNum);
            endNum = input.IndexOf(end, StringComparison.Ordinal);
            input = input.Remove(endNum);
            return input;
        }
    }
}

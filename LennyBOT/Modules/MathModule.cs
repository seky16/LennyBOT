// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
// ReSharper disable StyleCop.SA1126
namespace LennyBOT.Modules
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using FixerSharp;

    using LennyBOT.Config;
    using LennyBOT.Extensions;

    using Newtonsoft.Json.Linq;

    [Name("Math")]
    public class MathModule : LennyBase
    {
        [Command("calculate", RunMode = RunMode.Async), Alias("calc", "math")]
        [Remarks("Use math.js to evaluate expression. See http://mathjs.org/docs/ for more information.")]
        [MinPermissions(AccessLevel.User)]
        public async Task CalculateAsync([Remainder]string expression)
        {
            expression = expression.Replace("`", string.Empty);
            var url = $"http://api.mathjs.org/v1/?expr={Uri.EscapeDataString(expression)}";

            using (var client = new HttpClient())
            {
                var get = await client.GetAsync(url);
                if (!get.IsSuccessStatusCode)
                {
                    await this.ReplyAsync("http://api.mathjs.org/ couldn't be reached.");
                    return;
                }

                var result = await get.Content.ReadAsStringAsync();
                await this.ReplyAsync($"`{expression} = {result}`");
            }

            /*catch (Exception)
            {
                ////await this.ReplyAsync("```fix\nSomething went wrong```");
                await this.ReactAsync(Fail);
            }*/
        }

        [Command("convert", RunMode = RunMode.Sync), Alias("conv")]
        [Remarks("Currency converter (http://fixer.io/)")]
        [MinPermissions(AccessLevel.User)]
        public async Task ConvertAsync(string amount, string from, string to)
        {
            amount = amount.Replace(',', '.');
            if (!double.TryParse(amount, out var amountD))
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:"));
                return;
            }

            var author = this.Context.Message.Author as SocketGuildUser;
            var nick = author?.Nickname ?? this.Context.Message.Author.Username;
            var result = await Fixer.ConvertAsync(from, to, amountD);
            result = Math.Round(result, 2);
            var builder = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithName(nick ?? string.Empty).WithIconUrl(author?.GetAvatarUrl() ?? string.Empty))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await this.ReplyAsync(string.Empty, false, builder.Build());
        }

        [Command("convert", RunMode = RunMode.Sync), Alias("conv")]
        public async Task ConvertAsync(string amount, string from, string bullshitCheck, string to)
        {
            if (bullshitCheck.ToLowerInvariant() != "to")
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:"));
                return;
            }

            amount = amount.Replace(',', '.');
            if (!double.TryParse(amount, out var amountD))
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:"));
                return;
            }

            var author = this.Context.Message.Author as SocketGuildUser;
            var nick = author?.Nickname ?? this.Context.Message.Author.Username;
            var result = await Fixer.ConvertAsync(from, to, amountD);
            result = Math.Round(result, 2);
            var builder = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithName(nick ?? string.Empty).WithIconUrl(author?.GetAvatarUrl() ?? string.Empty))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await this.ReplyAsync(string.Empty, false, builder.Build());
        }
    }
}
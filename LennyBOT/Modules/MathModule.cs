// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using FixerSharp;

    using LennyBOT.Config;

    [Name("Math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("calculate", RunMode = RunMode.Async), Alias("calc", "math")]
        [Remarks("Calculate simple equations")]
        [MinPermissions(AccessLevel.User)]
        public async Task CalculateAsync([Remainder]string equation)
        { // Needs improvement
            // Replaces all the possible math symbols that may appear
            // Invalid for the computer to compute
            equation = equation.Trim('`').ToUpper()
            .Replace("×", "*")
            .Replace("X", "*")
            .Replace("÷", "/")
            .Replace("MOD", "%")
            .Replace("PI", "3.14159265359")
            .Replace("E", "2.718281828459045");
            try
            {
                var value = new DataTable().Compute(equation, null).ToString();
                if (value == "NaN")
                {
                    await this.ReplyAsync("`Infinity or undefined`").ConfigureAwait(false);
                }
                else
                {
                    await this.ReplyAsync($"`{value}`").ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                await this.ReplyAsync("```fix\nSomething went wrong```").ConfigureAwait(false);
            }
        }

        [Command("convert", RunMode = RunMode.Sync), Alias("conv")]
        [Remarks("Currency converter (http://fixer.io/)")]
        [MinPermissions(AccessLevel.User)]
        public async Task ConvertAsync(string amount, string from, string to)
        {
            amount = amount.Replace(',', '.');
            // ReSharper disable StyleCop.SA1126
            if (!double.TryParse(amount, out var amountD))
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:")).ConfigureAwait(false);
                return;
            }

            var author = this.Context.Message.Author as SocketGuildUser;
            var nick = author?.Nickname ?? this.Context.Message.Author.Username;
            var result = await Fixer.ConvertAsync(from, to, amountD).ConfigureAwait(false);
            result = Math.Round(result, 2);
            var builder = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithName(nick ?? string.Empty).WithIconUrl(author?.GetAvatarUrl() ?? string.Empty))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await this.ReplyAsync(string.Empty, false, builder.Build()).ConfigureAwait(false);
        }

        [Command("convert", RunMode = RunMode.Sync), Alias("conv")]
        public async Task ConvertAsync(string amount, string from, string bullshitCheck, string to)
        {
            if (bullshitCheck.ToLowerInvariant() != "to")
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:")).ConfigureAwait(false);
                return;
            }

            amount = amount.Replace(',', '.');
            // ReSharper disable StyleCop.SA1126
            if (!double.TryParse(amount, out var amountD))
            {
                await this.ReactAsync(EmojiExtensions.FromText(":question:")).ConfigureAwait(false);
                return;
            }

            var author = this.Context.Message.Author as SocketGuildUser;
            var nick = author?.Nickname ?? this.Context.Message.Author.Username;
            var result = await Fixer.ConvertAsync(from, to, amountD).ConfigureAwait(false);
            result = Math.Round(result, 2);
            var builder = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithName(nick ?? string.Empty).WithIconUrl(author?.GetAvatarUrl() ?? string.Empty))
                .WithCurrentTimestamp()
                .WithDescription($"{amountD.ToString(CultureInfo.InvariantCulture)} {from.ToUpper()} = **{result.ToString(CultureInfo.InvariantCulture)} {to.ToUpper()}**");
            await this.ReplyAsync(string.Empty, false, builder.Build()).ConfigureAwait(false);
        }

        private Task ReactAsync(IEmote emoji)
            => this.Context.Message.AddReactionAsync(emoji);
    }
}
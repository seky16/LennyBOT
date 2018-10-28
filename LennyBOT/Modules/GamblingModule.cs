// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
/*/ ReSharper disable StyleCop.SA1126
namespace LennyBOT.Modules
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.EmojiTools;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Models;
    using LennyBOT.Services;

    public class GamblingModule : LennyBase
    {
        public GamblingModule(RandomService random, ShekelsService shekels)
        {
            this.Random = random;
            this.Shekels = shekels;
        }

        private ShekelsService Shekels { get; }

        private RandomService Random { get; }

        [Command("check all"), Alias("shekels all", "leaderboard"), Remarks("")]
        public async Task LeaderboardAsync()
        {
            this.Shekels.Context = this.Context;
            var players = await this.Shekels.DownloadAsync();
            players = players.OrderByDescending(p => p.Shekels).ToList();
            var stringBuilder = new StringBuilder();
            var i = 1;
            foreach (var p in players)
            {
                var u = this.Context.Guild.GetUser(p.Id);
                if (u == null)
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        stringBuilder.Append($"{EmojiExtensions.FromText(":first_place:")} ");
                        break;
                    case 2:
                        stringBuilder.Append($"{EmojiExtensions.FromText(":second_place:")} ");
                        break;
                    case 3:
                        stringBuilder.Append($"{EmojiExtensions.FromText(":third_place:")} ");
                        break;
                    default:
                        stringBuilder.Append($"{i}. ");
                        break;
                }

                stringBuilder.AppendLine($"**{GetNickname(u)}**: {p.Shekels} Shekels");
                i++;
            }

            var embedB = new EmbedBuilder().WithCurrentTimestamp().WithColor(Color.Gold)
                .WithDescription(stringBuilder.ToString()).WithThumbnailUrl(
                    "https://new4.fjcdn.com/comments/Gt+yfw+you+realize+you+can+trick+your+drug+dealer+_b143ce67137a8d88d6829f20a4b35073.png");
            await this.ReplyAsync(string.Empty, false, embedB.Build());
        }

        [Command("check"), Alias("shekels"), Remarks("")]
        public async Task CheckShekelsAsync([Remainder]SocketGuildUser user = null)
        {
            this.Shekels.Context = this.Context;
            user = user ?? this.Context.User as SocketGuildUser;
            if (user == null)
            {
                await this.ReactAsync(Question);
                return;
            }

            var player = await this.Shekels.GetPlayerAsync(user);
            await this.ReplyAsync($"**{GetNickname(user)}**: {player.Shekels} Shekels");
        }

        [Command("slots all"), Remarks("")]
        public async Task PlaySlotsAllAsync()
        {
            this.Shekels.Context = this.Context;
            var player = await this.Shekels.GetPlayerAsync(this.Context.User);
            await this.PlaySlotsAsync(player.Shekels);
        }

        [Command("slots"), Remarks("")]
        public async Task PlaySlotsAsync(int inputCoins)
        {
            this.Shekels.Context = this.Context;
            var player = await this.Shekels.GetPlayerAsync(this.Context.User);
            if (!player.HasEnough(inputCoins))
            {
                await this.ReactAsync(Fail);
                return;
            }

            var slotEmotesCount = Slots.SlotEmotes.Count - 1;
            int one = this.Random.Generate(0, slotEmotesCount), two = this.Random.Generate(0, slotEmotesCount), three = this.Random.Generate(0, slotEmotesCount);

            var sb = new StringBuilder()
                .Append("**[  :slot_machine: | SLOTS ]**\n")
                .Append("------------------\n")
                .Append(
                    $"{Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]} : {Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]} : {Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]}\n")
                .Append($"{Slots.SlotEmotes[one]} : {Slots.SlotEmotes[two]} : {Slots.SlotEmotes[three]} < \n")
                .Append(
                    $"{Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]} : {Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]} : {Slots.SlotEmotes[this.Random.Generate(0, slotEmotesCount)]}\n")
                .Append("------------------\n");

            if (one == two && two == three)
            {
                sb.Append("| : : : : **WIN** : : : : |\n\n");
                var coinsWon = inputCoins * 2;
                sb.Append(
                    $"**{GetNickname(this.Context.User)}** bet **{inputCoins}** coin(s) and won **{coinsWon}** coin(s).\n");
                await this.Shekels.AddShekelsToPlayerAsync(player, coinsWon);
            }
            else if (one == two || two == three || one == three)
            {
                sb.Append("| : : : : **WIN** : : : : |\n\n");
                var coinsWon = inputCoins;
                sb.Append(
                    $"**{GetNickname(this.Context.User)}** bet **{inputCoins}** coin(s) and won **{coinsWon}** coin(s).\n");
                await this.Shekels.AddShekelsToPlayerAsync(player, coinsWon);
            }
            else
            {
                sb.Append("| : : :  **LOST**  : : : |\n\n");
                sb.Append($"**{GetNickname(this.Context.User)}** bet **{inputCoins}** coin(s) and lost.\n");
                await this.Shekels.AddShekelsToPlayerAsync(player, -inputCoins);
            }

            sb.Append($"Current balance: {player.Shekels} shekels");
            await this.ReplyAsync(sb.ToString());
        }
    }
}
////*/
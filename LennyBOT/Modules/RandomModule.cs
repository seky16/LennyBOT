// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System.Threading.Tasks;

    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Services;

    [Name("Random events")]
    public class RandomModule : LennyBase
    {
        [Command("random"), Alias("rand")]
        [Remarks("Generate TRUE random number between two values. (Info: http://stackoverflow.com/a/37804448)")]
        [MinPermissions(AccessLevel.User)]
        public Task RandCmdAsync(int from = 1, int to = 100)
        {
            var r = RandomService.Generate(from, to);
            return this.ReplyAsync(r.ToString());
        }

        [Command("coin"), Alias("flip")]
        [Remarks("Flips a coin")]
        [MinPermissions(AccessLevel.User)]
        public Task CoinCmdAsync()
        {
            var r = RandomService.Generate(1, 2);
            string res;
            switch (r)
            {
                case 1:
                    res = "Heads";
                    break;
                case 2:
                    res = "Tails";
                    break;
                default:
                    res = "err";
                    break;
            }

            return this.ReplyAsync(res);
        }

        [Command("choose"), Alias("decide")]
        [Remarks("Let the RNGesus decide for you")]
        [MinPermissions(AccessLevel.User)]
        public Task DecideCmdAsync(params string[] args)
        {
            var r = RandomService.Generate(0, args.Length - 1);
            return this.ReplyAsync(args[r]);
        }

        [Command("dice"), Alias("roll")]
        [Remarks("Roll number of dices with chosen number of sides. (Default sides = 6)")]
        [MinPermissions(AccessLevel.User)]
        public async Task DiceCmdAsync(int number = 1, int sides = 6)
        {
            if (number <= 0)
            {
                await this.ReplyAsync("Number of throws cannot be less or equal than 0.");
            }
            else if (sides <= 0)
            {
                await this.ReplyAsync("Number of sides cannot be less or equal than 0");
            }
            else
            {
                string res = null;
                var sum = 0;
                for (var i = 0; i < number; i++)
                {
                    var roll = RandomService.Generate(1, sides);
                    res += " " + roll;
                    sum += roll;
                }

                await this.ReplyAsync($"Rolled:{res}\nSum: {sum}");
            }
        }
    }
}
// ReSharper disable StyleCop.SA1600
using System.Threading.Tasks;

using Discord.Commands;

using LennyBOT.Config;
using LennyBOT.Services;

namespace LennyBOT.Modules
{
    [Name("Random events")]
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        private readonly RandomService _service;

        public RandomModule(RandomService service)
        {
            this._service = service;
        }

        [Command("random"), Alias("rand")]
        [Remarks("Generate TRUE random number between two values. (Info: http://stackoverflow.com/a/37804448)")]
        [MinPermissions(AccessLevel.User)]
        public async Task RandCmd(int from = 1, int to = 100)
        {
            var r = this._service.Between(from, to);
            await this.ReplyAsync(r.ToString());
        }

        [Command("coin"), Alias("flip")]
        [Remarks("Flips a coin")]
        [MinPermissions(AccessLevel.User)]
        public async Task CoinCmd()
        {
            var r = this._service.Between(1, 2);
            string res = null;
            if (r == 1) { res = "Heads"; }
            else if (r == 2) { res = "Tails"; }
            else { res = "err"; }
            await this.ReplyAsync(res);
        }

        [Command("choose"), Alias("decide")]
        [Remarks("Flips a coin")]
        [MinPermissions(AccessLevel.User)]
        public async Task DecideCmd(params string[] args)
        {
            var r = this._service.Between(0, args.Length - 1);
            await this.ReplyAsync(args[r]);
        }

        [Command("dice"), Alias("roll")]
        [Remarks("Roll number of dices with chosen number of sides. (Default sides = 6)")]
        [MinPermissions(AccessLevel.User)]
        public async Task DiceCmd(int number = 1, int sides = 6)
        {
            if (number <= 0)
            {
                await this.ReplyAsync("Number of throws cannot be less or equal than 0.");
            }
            else if (sides <= 0)
            {
                await this.ReplyAsync("Number of sides cannot be less or equal than 0");
            }
            else {

            int roll;
            string res = null;
            var sum = 0;

            for (var i = 0; i < number; i++)
            {
                roll = this._service.Between(1, sides);
                res += " " + roll.ToString();
                sum += roll;
            }

            await this.ReplyAsync($"Rolled:{res}\nSum: {sum}");
        }}
    }
}
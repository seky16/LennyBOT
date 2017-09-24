// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Models
{
    using Discord;

    public class Player
    {
        public Player(ulong id, int shekels)
        {
            this.Id = id;
            this.Shekels = shekels;
        }

        public Player(IUser user, int shekels)
        {
            this.Id = user.Id;
            this.Shekels = shekels;
        }

        public ulong Id { get; }

        public int Shekels { get; set; }

        public void AddShekels(int amount)
        {
            this.Shekels = this.Shekels + amount;
        }

        public void RemoveShekels(int amount)
        {
            this.Shekels = this.Shekels - amount;
        }
    }
}

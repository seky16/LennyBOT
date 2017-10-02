// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Models
{
    public class Player
    {
        public Player(ulong id, int shekels)
        {
            this.Id = id;
            this.Shekels = shekels;
        }

        public ulong Id { get; }

        public int Shekels { get; set; }

        public void AddShekels(int amount)
        {
            this.Shekels = this.Shekels + amount;
        }

        /*public void RemoveShekels(int amount)
        {
            this.Shekels = this.Shekels - amount;
        }*/

        public bool HasEnough(int shekels)
        {
            return shekels > 0 && this.Shekels > 0 && this.Shekels >= shekels;
        }
    }
}

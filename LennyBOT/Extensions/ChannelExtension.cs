// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Extensions
{
    using System.Linq;
    using System.Threading.Tasks;

    using Discord;

    public static class ChannelExtension
    {
        public static async Task<IMessage> GetLastMessageAsync(this ITextChannel channel)
        {
            var msgEnum = await channel.GetMessagesAsync(1).Flatten();
            var msg = msgEnum.FirstOrDefault();
            return msg;
        }
    }
}

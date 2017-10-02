// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Extensions
{
    using System.Threading.Tasks;

    using Discord;
    using Discord.WebSocket;

    public static class MessageExtention
    {
        public static IMessage DeleteAfter(this IUserMessage msg, int seconds)
        {
            Task.Run(async () =>
                {
                    await Task.Delay(seconds * 1000).ConfigureAwait(false);
                    try
                    {
                        await msg.DeleteAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            return msg;
        }

        public static IMessage ModifyAfter(this IUserMessage msg, string message, int seconds)
        {
            Task.Run(async () =>
                {
                    await Task.Delay(seconds * 1000).ConfigureAwait(false);
                    try
                    {
                        await msg.ModifyAsync(x => x.Content = message).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            return msg;
        }
    }
}

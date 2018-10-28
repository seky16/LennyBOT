// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Extensions
{
    using System.Threading.Tasks;

    using Discord;

    public static class MessageExtention
    {
        public static IMessage DeleteAfter(this IUserMessage msg, int seconds)
        {
            Task.Run(async () =>
                {
                    await Task.Delay(seconds * 1000);
                    try
                    {
                        await msg.DeleteAsync();
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
                    await Task.Delay(seconds * 1000);
                    try
                    {
                        await msg.ModifyAsync(x => x.Content = message);
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
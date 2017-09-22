// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System.Threading.Tasks;

    using Discord.Commands;

    using LennyBOT.Config;
    using LennyBOT.Services;

    [Name("Player")]
    public class PlayerModule : ModuleBase<SocketCommandContext>
    {
        private readonly ShekelsService shekels;

        public PlayerModule(ShekelsService shekels)
        {
            this.shekels = shekels;
        }

        [Command("play", RunMode = RunMode.Async), Alias("p")]
        [Remarks("")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PlayCmdAsync()
        {
            await this.shekels.DownloadAsync(this.Context).ConfigureAwait(false);
            var p = this.shekels.GetPlayer(123);

            // await _service.Upload(Context, Export());
            p.AddShekels(10);
            this.shekels.AddOrUpdatePlayer(p);
            await ShekelsService.UploadAsync(this.Context, this.shekels.Export()).ConfigureAwait(false);
            p.RemoveShekels(5);
            this.shekels.AddOrUpdatePlayer(p);
            await ShekelsService.UploadAsync(this.Context, this.shekels.Export()).ConfigureAwait(false);

            // await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");
            // p.RemoveShekels(5);
            // await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");
        }
    }
}
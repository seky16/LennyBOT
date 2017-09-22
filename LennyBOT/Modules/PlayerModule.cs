// ReSharper disable StyleCop.SA1600
using System.Threading.Tasks;

using Discord.Commands;

using LennyBOT.Config;
using LennyBOT.Services;

namespace LennyBOT.Modules
{
    [Name("Player")]
    public class PlayerModule : ModuleBase<SocketCommandContext>
    {
        private readonly ShekelsService _shekels;

        public PlayerModule(ShekelsService shekels)
        {
            this._shekels = shekels;
        }

        [Command("play", RunMode = RunMode.Async), Alias("p")]
        [Remarks("")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PlayCmd()
        {
            await this._shekels.DownloadAsync(this.Context);
            var p = this._shekels.GetPlayer(123);

            // await _service.Upload(Context, Export());
            p.AddShekels(10);
            this._shekels.AddOrUpdatePlayer(p);
            await ShekelsService.UploadAsync(this.Context, this._shekels.Export());
            p.RemoveShekels(5);
            this._shekels.AddOrUpdatePlayer(p);
            await ShekelsService.UploadAsync(this.Context, this._shekels.Export());

            // await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");
            // p.RemoveShekels(5);
            // await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");
        }
    }
}

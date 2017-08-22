using Discord.Commands;
using LennyBOT.Config;
using LennyBOT.Services;
using System.Threading.Tasks;

namespace LennyBOT.Modules
{
    [Name("Player")]
    public class PlayerModule : ModuleBase<SocketCommandContext>
    {
        private readonly ShekelsService _shekels;

        public PlayerModule(ShekelsService shekels)
        {
            _shekels = shekels;
        }

        [Command("play", RunMode = RunMode.Async), Alias("p")]
        [Remarks("")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task PlayCmd()
        {
            await _shekels.Download(Context);
            var p = _shekels.GetPlayer(123);
            //await _service.Upload(Context, Export());
            p.AddShekels(10);
            _shekels.AddOrUpdatePlayer(p);
            await _shekels.Upload(Context, _shekels.Export());
            p.RemoveShekels(5);
            _shekels.AddOrUpdatePlayer(p);
            await _shekels.Upload(Context, _shekels.Export());

            //await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");
            //p.RemoveShekels(5);
            //await ReplyAsync($"id: {p.Id}; \nshekels: {p.Shekels};");

        }

    }
}

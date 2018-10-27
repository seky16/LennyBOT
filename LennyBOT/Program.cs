// ReSharper disable StyleCop.SA1600
namespace LennyBOT
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    using LennyBOT.Config;
    using LennyBOT.Services;

    using Microsoft.Extensions.DependencyInjection;

    internal class Program
    {
        private readonly DiscordSocketClient client;

        // Keep the CommandService and IServiceCollection around for use with commands.
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        private IServiceProvider services;

        private Program()
        {
            this.client = new DiscordSocketClient(new DiscordSocketConfig
                                                      {
                                                          // How much logging do you want to see?
                                                          LogLevel = LogSeverity.Info,

                                                          // If you or another service needs to do anything with messages
                                                          // (eg. checking Reactions, checking the content of edited/deleted messages),
                                                          // you must set the MessageCacheSize. You may adjust the number as needed.
                                                          MessageCacheSize = 1000,

                                                          // If your platform doesn't have native websockets,
                                                          // add Discord.Net.Providers.WS4Net from NuGet,
                                                          // add the `using` at the top, and uncomment this line:
                                                          // WebSocketProvider = WS4NetProvider.Instance
                                                      });
        }

        // Program entry point
        internal static void Main() =>

            // Call the Program constructor, followed by the
            // MainAsync method and wait until it finishes (which should be never).
            new Program().MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        private static Task LoggerAsync(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                // case LogSeverity.Verbose:
                // case LogSeverity.Debug:
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now, -19} [{message.Severity, 8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            // Subscribe the logging handler.
            this.client.Log += LoggerAsync;
            this.commands.Log += LoggerAsync;

            // Centralize the logic for commands into a seperate method.
            await this.InitCommandsAsync().ConfigureAwait(false);

            // Ensure the configuration file has been created.
            Configuration.EnsureExists();

            // Login and connect.
            Console.WriteLine($"prefix: {prefix}");
            await this.client.LoginAsync(TokenType.Bot, Configuration.Load().Token).ConfigureAwait(false);
            await this.client.StartAsync().ConfigureAwait(false);

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task InitCommandsAsync()
        {
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            // _map.AddSingleton(new SomeServiceClass());
            Console.WriteLine("add services");
            this.map.AddSingleton(this.client);
            this.map.AddSingleton(new FactService());
            this.map.AddSingleton(new ShekelsService());
            this.map.AddSingleton(new RandomService());
            this.map.AddSingleton(new SearchService());
            this.map.AddSingleton(new TagService());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            Console.WriteLine("build services");
            this.services = this.map.BuildServiceProvider();

            // Either search the program and add all Module classes that can be found.
            // Module classes *must* be marked 'public' or they will be ignored.
            Console.WriteLine("add modules");
            await this.commands.AddModulesAsync(Assembly.GetEntryAssembly(), services).ConfigureAwait(false);

            // Or add Modules manually if you prefer to be a little more explicit:
            // await _commands.AddModuleAsync<SomeModule>();

            // Subscribe a handler to see if a message invokes a command.
            Console.WriteLine("subscribe");
            this.client.MessageReceived += this.HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            Console.WriteLine("Got socket msg");
            // Bail out if it's a System Message.
            // ReSharper disable once UsePatternMatching
            var msg = arg as SocketUserMessage;
            if (msg == null)
            {
                Console.WriteLine("not SocketUserMsg");
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            var pos = 0;

            var prefix = Configuration.Load().Prefix;
            if (msg.HasCharPrefix(prefix, ref pos) || msg.HasMentionPrefix(this.client.CurrentUser, ref pos))
            {
                // Create a Command Context.
                Console.WriteLine("create ctxt");
                var context = new SocketCommandContext(this.client, msg);

                // Execute the command. (result does not indicate a return value,
                // rather an object stating if the command executed succesfully).
                Console.WriteLine("execCmd");
                var result = await this.commands.ExecuteAsync(context, pos, this.services).ConfigureAwait(false);
                Console.WriteLine($"result.Error.HasValue={result.Error.HasValue}\nresult.Error.Value={result.Error.Value}");
                Console.WriteLine(result.ToString());
                // Uncomment the following lines if you want the bot
                // to send a message if it failed (not advised for most situations).
                // if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                // await msg.Channel.SendMessageAsync(result.ErrorReason);
                if (result.Error.HasValue && result.Error.Value == CommandError.UnknownCommand)
                {
                    Console.WriteLine("hasValue && value==unknown");
                    return;
                }

                if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand)
                {
                    Console.WriteLine("sendMsg");
                    await context.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
                }

                // ^ from foxbot's DiscordBotBase https://github.com/foxbot/DiscordBotBase/tree/csharp+data
            }
        }
    }
}

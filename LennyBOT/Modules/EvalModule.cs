// ReSharper disable StyleCop.SA1600
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LennyBOT.Config;
using LennyBOT.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Reflection;

namespace LennyBOT.Modules
{
    [Name("Eval")]
    public class EvalModule : ModuleBase<SocketCommandContext>
    {
        private ScriptOptions scriptOptions;
        private void CreateScriptOptions()
        {
            // mscorlib reference issues when using codeanalysis;
            // see http://stackoverflow.com/questions/38943899/net-core-cs0012-object-is-defined-in-an-assembly-that-is-not-referenced
            var dd = typeof(object).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile($"{coreDir.FullName}{Path.DirectorySeparatorChar}mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            };

            var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var referencedAssembly in referencedAssemblies)
            {
                var loadedAssembly = Assembly.Load(referencedAssembly);
                references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
            }

            this.scriptOptions = ScriptOptions.Default.
                AddImports("System", "System.Linq", "System.Text", "Discord", "Discord.WebSocket").
                AddReferences(references);
        }

        [Command("eval", RunMode = RunMode.Async)]
        [Remarks("Evaluate code")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task EvalCmd([Remainder]string script)
        {
            CreateScriptOptions();
            var message = Context.Message;
            string result = "no result";
            try
            {
                var evalResult = await CSharpScript.EvaluateAsync<object>(script, scriptOptions, globals: new ScriptHost { Message = message, Client = Context.Client });
                result = evalResult.ToString();
            }
            catch (Exception ex)
            {
                result = ex.ToString().Substring(0, Math.Min(ex.ToString().Length, 800));
            }

            await ReplyAsync($"``{result}``");
        }
    }
    public class ScriptHost
    {
        public SocketMessage Message { get; set; }
        public DiscordSocketClient Client { get; set; }
    }
}*/
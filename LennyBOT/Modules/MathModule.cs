// ReSharper disable StyleCop.SA1600
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using LennyBOT.Config;
using LennyBOT.Services;

namespace LennyBOT.Modules
{
    [Name("Math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("calculate", RunMode = RunMode.Async), Alias("calc", "math")]
        public async Task Calculate([Remainder]string equation)
        { // Needs improvement
            // Replaces all the possible math symbols that may appear
            // Invalid for the computer to compute
            equation = equation.Trim('`').ToUpper()
            .Replace("×", "*")
            .Replace("X", "*")
            .Replace("÷", "/")
            .Replace("MOD", "%")
            .Replace("PI", "3.14159265359")
            .Replace("E", "2.718281828459045");
            try
            {
                var value = new DataTable().Compute(equation, null).ToString();
                if (value == "NaN")
                {
                    await this.ReplyAsync("`Infinity or undefined`");
                }
                else
                {
                    await this.ReplyAsync($"`{value}`");
                }
            }
            catch (Exception)
            {
                await this.ReplyAsync($"```fix\nSomething went wrong```");
            }
        }


    }
}

// ReSharper disable StyleCop.SA1600
// ReSharper disable UnusedMember.Global
namespace LennyBOT.Modules
{
    using System;
    using System.Data;
    using System.Threading.Tasks;

    using Discord.Commands;

    [Name("Math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("calculate", RunMode = RunMode.Async), Alias("calc", "math")]
        public async Task CalculateAsync([Remainder]string equation)
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
                    await this.ReplyAsync("`Infinity or undefined`").ConfigureAwait(false);
                }
                else
                {
                    await this.ReplyAsync($"`{value}`").ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                await this.ReplyAsync("```fix\nSomething went wrong```").ConfigureAwait(false);
            }
        }
    }
}
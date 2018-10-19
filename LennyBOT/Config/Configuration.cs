// ReSharper disable StyleCop.SA1600
namespace LennyBOT.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// A file that contains information you either don't want public
    /// or will want to change without having to compile another bot.
    /// </summary>
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    public class Configuration
    {
        /// <summary> Gets or sets Ids of users who will have owner access to the bot. </summary>
        public IEnumerable<ulong> Owners { get; set; } = null;

        /// <summary> Gets or sets your bot's command prefix. </summary>
        public char Prefix { get; set; } = '?';

        /// <summary> Gets or sets your bot's login token. </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string Token { get; set; } = string.Empty;

        /// <summary> Gets or sets Steam API key. </summary>
        public string SteamApiKey { get; set; } = string.Empty;

        /// <summary> Gets or sets Osu API key. </summary>
        public string OsuApiKey { get; set; } = string.Empty;

        /// <summary> Gets the path of your bot's configuration file. </summary>
        [JsonIgnore]
        private static string FileName { get; } = "Files/test.json";

        /// <summary>
        /// Ensures the config file exists, if not it creates it.
        /// </summary>
        public static void EnsureExists()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                // Check if the configuration file exists.
                var path = Path.GetDirectoryName(file);          // Create config directory if doesn't exist.
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var config = new Configuration();                   // Create a new configuration object.

                Console.WriteLine("Please enter your token: ");
                var token = Console.ReadLine();                  // Read the bot token from console.

                config.Token = token;
                config.SaveJson();                                  // Save the new configuration object to file.
            }

            Console.WriteLine("Configuration Loaded");
        }

        /// <summary>
        /// Load the configuration from the path specified in FileName.
        /// </summary>
        /// <returns>
        /// The <see cref="Configuration"/>.
        /// </returns>
        public static Configuration Load()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
        }

        /// <summary> Save the configuration to the path specified in FileName. </summary>
        private void SaveJson()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, this.ToJson());
        }

        /// <summary>
        /// Convert the configuration to a json string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
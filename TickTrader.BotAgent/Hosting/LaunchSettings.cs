using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Hosting
{
    public enum LaunchMode
    {
        Console = 0,
        WindowsService = 1,
    }

    public class LaunchSettings
    {
        public const string EnvironmentKey = "environment";
        public const string EnvVarEnvironmentKey = "aspnetcore_environment";
        public const string ConsoleKey = "console";


        public string Environment { get; set; }

        public LaunchMode Mode { get; set; }


        public override string ToString()
        {
            return $"Launch {Mode} in {Environment} env";
        }


        public static LaunchSettings Read(string[] args, Dictionary<string, string> switchMappings)
        {
            var res = new LaunchSettings { Mode = LaunchMode.WindowsService };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            configBuilder.AddCommandLine(args, switchMappings);

            var config = configBuilder.Build();
            res.Environment = EnvironmentAliases.ResolveEnvironemntAlias(config[EnvironmentKey] ?? config[EnvVarEnvironmentKey]);

            if (EnvironmentAliases.IsDevelopment(res.Environment) || EnvironmentAliases.IsStaging(res.Environment) || config[ConsoleKey] != null)
                res.Mode = LaunchMode.Console;

            return res;
        }


        public Dictionary<string, string> GenerateEnvironmentOverride()
        {
            return new Dictionary<string, string> { { EnvironmentKey, Environment } };
        }
    }
}

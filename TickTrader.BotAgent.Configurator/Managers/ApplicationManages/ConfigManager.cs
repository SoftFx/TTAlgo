using System.Collections.Generic;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public static class ConfigurationProperies
    {
        public static string AppSettings => Path.Combine("WebAdmin", "appsettings.json");

        public static List<(string Type, string LogFile)> RegistryApplicationNames => new List<(string, string)>
        {
            ("AlgoServer", "server.log"),
            ("BotAgent", "agent.log"),
        };

        public static bool IsDeveloper => true;
    }
}

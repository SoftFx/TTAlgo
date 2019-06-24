using System;
using System.IO;
using System.Reflection;
using TickTrader.BotAgent.Common.Extensions;

namespace TickTrader.BotAgent.Configurator
{
    public class AgentVersionManager
    {
        private readonly string _agentPath;

        public string Version { get; }
        public string BuildDate { get; }
        public string FullVersion => $"{Version} ({BuildDate})";

        public AgentVersionManager(string agentPath, string fileName)
        {
            _agentPath = Path.Combine(agentPath, $"{fileName}.exe");

            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(_agentPath);
                Version = assemblyName.Version.ToString();
                BuildDate = assemblyName.GetLinkerTime().ToString("yyyy.MM.dd"); 
            }
            catch (Exception ex)
            { }    
        }
    }
}

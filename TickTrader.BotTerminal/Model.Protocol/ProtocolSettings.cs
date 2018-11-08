using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class ProtocolSettings : IProtocolSettings
    {
        public int ListeningPort { get; set; }

        public string LogDirectoryName { get; set; }

        public bool LogMessages { get; set; }


        public ProtocolSettings()
        {
            var cfgSection = ProtocolConfigSection.GetCfgSection();
            var logsConfig = cfgSection.Logs;

            LogDirectoryName = EnvService.Instance.LogFolder;
            LogMessages = logsConfig.LogMessages;
        }
    }
}

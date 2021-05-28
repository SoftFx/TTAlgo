namespace TickTrader.BotTerminal
{
    internal static class ProtocolSettings
    {
        public static string LogDirectoryName { get; private set; }

        public static bool LogMessages { get; private set; }


        static ProtocolSettings()
        {
            var cfgSection = ProtocolConfigSection.GetCfgSection();
            var logsConfig = cfgSection.Logs;

            LogDirectoryName = EnvService.Instance.LogFolder;
            LogMessages = logsConfig.LogMessages;
        }
    }
}

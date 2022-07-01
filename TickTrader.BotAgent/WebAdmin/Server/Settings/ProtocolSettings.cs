namespace TickTrader.BotAgent.WebAdmin.Server.Settings
{
    public class ProtocolSettings
    {
        public int ListeningPort { get; set; }

        public bool IsLocalServer { get; set; }

        public string LogDirectoryName { get; set; }

        public bool LogMessages { get; set; }
    }
}

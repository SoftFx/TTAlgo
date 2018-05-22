using TickTrader.Algo.Protocol.Sfx;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class ProtocolSettings : IProtocolSettings
    {
        public int ListeningPort { get; set; }

        public string LogDirectoryName { get; set; }

        public bool LogEvents { get; set; }

        public bool LogStates { get; set; }

        public bool LogMessages { get; set; }
    }
}

using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.CmdClient
{
    public class ProtocolClientSettings : IClientSessionSettings
    {
        public string ServerAddress { get; set; }

        public string ServerCertificateName { get; set; }

        public IProtocolSettings ProtocolSettings { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}

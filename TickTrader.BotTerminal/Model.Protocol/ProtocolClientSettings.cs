using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class ProtocolClientSettings : IClientSessionSettings
    {
        private ProtocolSettings _protocolSettings;


        public string ServerAddress { get; set; }

        public IProtocolSettings ProtocolSettings => _protocolSettings;

        public string Login { get; set; }

        public string Password { get; set; }

        public int Port { get => _protocolSettings.ListeningPort; set => _protocolSettings.ListeningPort = value; }


        public ProtocolClientSettings()
        {
            _protocolSettings = new ProtocolSettings();
        }
    }
}

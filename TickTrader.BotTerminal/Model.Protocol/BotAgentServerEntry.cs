using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    internal class BotAgentServerEntry
    {
        public string Name { get; }

        public string Address { get; }

        public int Port { get; }


        public BotAgentServerEntry(ProtocolServerElement cfgElement)
            : this(cfgElement.Name, cfgElement.Address, cfgElement.Port)
        {
        }

        public BotAgentServerEntry(string address)
            : this(address, address)
        {
        }

        public BotAgentServerEntry(string name, string address) :
            this(name, address, 8443)
        {
        }

        public BotAgentServerEntry(string name, string address, int port)
        {
            Name = name;
            Address = address;
            Port = port;

            if (string.IsNullOrWhiteSpace(name))
                Name = address;
        }
    }
}

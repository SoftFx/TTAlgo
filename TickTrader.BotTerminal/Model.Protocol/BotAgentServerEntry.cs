using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    internal class BotAgentServerEntry
    {
        public string Name { get; }

        public string Address { get; }

        public int Port { get; }

        public string CertificateName { get; }


        public BotAgentServerEntry(ProtocolServerElement cfgElement)
            : this(cfgElement.Name, cfgElement.Address, cfgElement.Port, cfgElement.CertificateName)
        {
        }

        public BotAgentServerEntry(string address)
            : this(address, address)
        {
        }

        public BotAgentServerEntry(string name, string address) :
            this(name, address, 8443, "certificate.pfx")
        {
        }

        public BotAgentServerEntry(string name, string address, int port, string certificateName)
        {
            Name = name;
            Address = address;
            Port = port;
            CertificateName = certificateName;

            if (string.IsNullOrWhiteSpace(name))
                Name = address;
        }
    }
}

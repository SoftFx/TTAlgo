using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    internal class BotAgentServerEntry
    {
        public string Name { get; }

        public string ShortName { get; }

        public string Address { get; }

        public int Port { get; }

        public string CertificateName { get; }

        public Color Color { get; }


        public BotAgentServerEntry(ProtocolServerElement cfgElement)
            : this(cfgElement.Name, cfgElement.ShortName, cfgElement.Address, cfgElement.Port, cfgElement.CertificateName, cfgElement.Color)
        {
        }

        public BotAgentServerEntry(string address)
            : this(address, address, address, null)
        {
        }

        public BotAgentServerEntry(string name, string shortName, string address, string color) :
            this(name, shortName, address, 8443, "certificate.pfx", null)
        {
        }

        public BotAgentServerEntry(string name, string shortName, string address, int port, string certificateName, string color)
        {
            Name = name;
            Address = address;
            ShortName = shortName;
            Port = port;
            CertificateName = certificateName;

            if (string.IsNullOrWhiteSpace(name))
                Name = address;

            if (string.IsNullOrWhiteSpace(ShortName))
                ShortName = name;

            if (!string.IsNullOrWhiteSpace(color))
                Color = (Color)ColorConverter.ConvertFromString(color);
        }
    }
}

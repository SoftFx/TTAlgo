using System.Security.Cryptography.X509Certificates;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class ProtocolServerSettings : IServerSettings
    {
        public string ServerName { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public IProtocolSettings ProtocolSettings { get; set; }
    }
}

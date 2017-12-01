using System.Security.Cryptography.X509Certificates;

namespace TickTrader.Algo.Protocol
{
    public interface IServerSettings
    {
        string ServerName { get; }

        X509Certificate2 Certificate { get; }

        IProtocolSettings ProtocolSettings { get; }

        string Login { get; }

        string Password { get; }
    }
}

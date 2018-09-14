using System.Security.Cryptography.X509Certificates;

namespace TickTrader.Algo.Protocol.Sfx
{
    public interface IServerSettings
    {
        string ServerName { get; }

        X509Certificate2 Certificate { get; }

        IProtocolSettings ProtocolSettings { get; }
    }
}

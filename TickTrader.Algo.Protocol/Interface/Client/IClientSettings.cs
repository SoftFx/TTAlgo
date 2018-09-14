namespace TickTrader.Algo.Protocol
{
    public interface IClientSessionSettings
    {
        string ServerAddress { get; }

        string ServerCertificateName { get; }

        IProtocolSettings ProtocolSettings { get; }

        string Login { get; }

        string Password { get; }
    }
}

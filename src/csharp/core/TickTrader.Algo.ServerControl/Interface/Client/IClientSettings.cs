namespace TickTrader.Algo.ServerControl
{
    public interface IClientSessionSettings
    {
        string ServerAddress { get; }

        IProtocolSettings ProtocolSettings { get; }

        string Login { get; }

        string Password { get; }
    }
}

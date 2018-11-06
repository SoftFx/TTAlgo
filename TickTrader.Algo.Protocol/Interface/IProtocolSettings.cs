namespace TickTrader.Algo.Protocol
{
    public interface IProtocolSettings
    {
        int ListeningPort { get; }

        string LogDirectoryName { get; }

        bool LogMessages { get; }
    }
}

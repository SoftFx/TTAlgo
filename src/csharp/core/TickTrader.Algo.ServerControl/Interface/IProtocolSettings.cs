namespace TickTrader.Algo.ServerControl
{
    public interface IProtocolSettings
    {
        int ListeningPort { get; }

        string LogDirectoryName { get; }

        bool LogMessages { get; }
    }
}

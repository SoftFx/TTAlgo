namespace TickTrader.Algo.Protocol
{
    public interface IProtocolSettings
    {
        int ListeningPort { get; }

        string LogDirectoryName { get; }

        bool LogEvents { get; }

        bool LogStates { get; }

        bool LogMessages { get; }
    }
}

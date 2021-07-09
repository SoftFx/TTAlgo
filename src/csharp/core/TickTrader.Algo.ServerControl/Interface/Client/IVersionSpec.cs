namespace TickTrader.Algo.ServerControl
{
    public interface IVersionSpec
    {
        int MajorVersion { get; }

        int MinorVersion { get; }

        int CurrentVersion { get; }
    }
}

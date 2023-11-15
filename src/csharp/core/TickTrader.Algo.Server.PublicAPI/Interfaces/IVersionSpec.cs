namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IVersionSpec
    {
        int MajorVersion { get; }

        int MinorVersion { get; }

        int CurrentVersion { get; }

        bool SupportsAutoUpdate { get; }
    }
}

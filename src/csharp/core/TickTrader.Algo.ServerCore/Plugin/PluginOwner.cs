namespace TickTrader.Algo.Server
{
    public static class PluginOwner
    {
        public record ExecPluginCmd(string PluginId, object Command);
    }
}

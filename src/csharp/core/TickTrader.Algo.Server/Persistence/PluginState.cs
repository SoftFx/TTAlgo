namespace TickTrader.Algo.Server.Persistence
{
    internal class PluginState
    {
        public string Id { get; set; }

        public bool IsRunning { get; set; }

        public string ConfigVersion { get; set; }

        public string Config { get; set; }
    }
}

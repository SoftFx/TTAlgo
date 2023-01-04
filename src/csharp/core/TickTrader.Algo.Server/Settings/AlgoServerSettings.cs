namespace TickTrader.Algo.Server
{
    public class AlgoServerSettings
    {
        public bool EnableAccountLogs { get; set; }

        public bool EnableIndicatorHost { get; set; }

        public AlgoHostSettings HostSettings { get; set; } = new AlgoHostSettings();

        public MonitoringSettings MonitoringSettings { get; } = new MonitoringSettings();
    }
}

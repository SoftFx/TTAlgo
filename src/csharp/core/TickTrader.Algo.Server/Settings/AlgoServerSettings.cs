namespace TickTrader.Algo.Server
{
    public class AlgoServerSettings
    {
        public string DataFolder { get; set; }

        public bool EnableAccountLogs { get; set; }

        public AlgoHostSettings HostSettings { get; set; } = new AlgoHostSettings();

        public MonitoringSettings MonitoringSettings { get; } = new MonitoringSettings();
    }
}

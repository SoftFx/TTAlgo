using TickTrader.Algo.PkgStorage;

namespace TickTrader.Algo.Server
{
    public class AlgoServerSettings
    {
        public string DataFolder { get; set; }

        public bool EnableAccountLogs { get; set; }

        public PkgStorageSettings PkgStorage { get; } = new PkgStorageSettings();

        public RuntimeSettings RuntimeSettings { get; } = new RuntimeSettings();

        public MonitoringSettings MonitoringSettings { get; } = new MonitoringSettings();
    }
}

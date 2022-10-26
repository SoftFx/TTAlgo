namespace TickTrader.Algo.Server
{
    public sealed class MonitoringSettings
    {
        public QuoteMonitoringSettings QuoteMonitoring { get; set; }


        public MonitoringSettings()
        {
            QuoteMonitoring = new QuoteMonitoringSettings
            {
                EnableMonitoring = false,
                AccetableQuoteDelay = 1000,
                AlertsDelay = 5,
                SaveOnDisk = true,
            };
        }
    }


    public sealed record QuoteMonitoringSettings
    {
        public bool EnableMonitoring { get; init; }

        public int AccetableQuoteDelay { get; init; }

        public int AlertsDelay { get; init; }

        public bool SaveOnDisk { get; init; }
    }
}
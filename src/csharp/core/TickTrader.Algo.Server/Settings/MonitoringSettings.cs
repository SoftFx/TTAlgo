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
            };
        }
    }


    public sealed class QuoteMonitoringSettings
    {
        public bool EnableMonitoring { get; set; }

        public int AccetableQuoteDelay { get; set; }

        public int AlertsDelay { get; set; }
    }
}
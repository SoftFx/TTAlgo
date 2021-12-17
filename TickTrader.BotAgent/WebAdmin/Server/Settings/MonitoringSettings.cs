using Newtonsoft.Json;

namespace TickTrader.BotAgent.WebAdmin.Server.Settings
{
    public class MonitoringSettings
    {
        public QuoteMonitoringSettings QuoteMonitoring { get; set; }
    }


    public class QuoteMonitoringSettings
    {
        public bool EnableMonitoring { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AccetableQuoteDelay { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AlertsDelay { get; set; }


        [JsonIgnore]
        internal static QuoteMonitoringSettings Default { get; } = new QuoteMonitoringSettings
        {
            EnableMonitoring = false,
            AccetableQuoteDelay = 60000,
            AlertsDelay = 1,
        };
    }
}
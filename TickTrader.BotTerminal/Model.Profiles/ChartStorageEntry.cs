using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Chart")]
    internal class ChartStorageEntry
    {
        [DataMember]
        public string Symbol { get; set; }

        [DataMember(Name = "Period")]
        public string SelectedPeriod { get; set; }

        [DataMember(Name = "ChartType")]
        public SelectableChartTypes SelectedChartType { get; set; }

        [DataMember]
        public bool CrosshairEnabled { get; set; }

        [DataMember]
        public List<IndicatorStorageEntry> Indicators { get; set; }

        [DataMember]
        public List<TradeBotStorageEntry> Bots { get; set; }


        public ChartStorageEntry()
        {
        }


        public ChartStorageEntry Clone()
        {
            return new ChartStorageEntry
            {
                Symbol = Symbol,
                SelectedPeriod = SelectedPeriod,
                SelectedChartType = SelectedChartType,
                CrosshairEnabled = CrosshairEnabled,
                Indicators = Indicators != null ? new List<IndicatorStorageEntry>(Indicators.Select(c => c.Clone())) : null,
                Bots = Bots != null ? new List<TradeBotStorageEntry>(Bots.Select(c => c.Clone())) : null,
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Chart")]
    internal class ChartStorageEntry
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Symbol { get; set; }

        [DataMember(Name = "Period")]
        public ChartPeriods SelectedPeriod { get; set; }

        [DataMember(Name = "ChartType")]
        public SelectableChartTypes SelectedChartType { get; set; }

        [DataMember]
        public bool CrosshairEnabled { get; set; }

        [DataMember]
        public List<IndicatorStorageEntry> Indicators { get; set; }


        public ChartStorageEntry()
        {
        }


        public ChartStorageEntry Clone()
        {
            return new ChartStorageEntry
            {
                Id = Id,
                Symbol = Symbol,
                SelectedPeriod = SelectedPeriod,
                SelectedChartType = SelectedChartType,
                CrosshairEnabled = CrosshairEnabled,
                Indicators = Indicators != null ? new List<IndicatorStorageEntry>(Indicators.Select(c => c.Clone())) : null,
            };
        }
    }
}

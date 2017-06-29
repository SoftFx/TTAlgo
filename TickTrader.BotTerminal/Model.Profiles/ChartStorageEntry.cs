using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Chart")]
    public class ChartStorageEntry
    {
        [DataMember]
        public string Symbol { get; set; }

        [DataMember(Name = "Period")]
        public string SelectedPeriod { get; set; }

        [DataMember(Name = "ChartType")]
        public SelectableChartTypes SelectedChartType { get; set; }

        [DataMember]
        public bool CrosshairEnabled { get; set; }


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
            };
        }
    }
}

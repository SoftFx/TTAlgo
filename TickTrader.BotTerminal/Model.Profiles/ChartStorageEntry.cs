using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Controls.Chart;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Chart")]
    internal class ChartStorageEntry
    {
        public enum ChartPeriods { MN1, W1, D1, H4, H1, M30, M15, M5, M1, S10, S1, Ticks };

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Symbol { get; set; }

        [DataMember(Name = "Period")]
        public ChartPeriods SelectedPeriod { get; set; }

        [DataMember(Name = "ChartType")]
        public ChartTypes SelectedChartType { get; set; }

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

        public static ChartPeriods ConvertPeriod(Feed.Types.Timeframe timeFrame)
        {
            return timeFrame switch
            {
                Feed.Types.Timeframe.MN => ChartPeriods.MN1,
                Feed.Types.Timeframe.D => ChartPeriods.D1,
                Feed.Types.Timeframe.W => ChartPeriods.W1,
                Feed.Types.Timeframe.H4 => ChartPeriods.H4,
                Feed.Types.Timeframe.H1 => ChartPeriods.H1,
                Feed.Types.Timeframe.M30 => ChartPeriods.M30,
                Feed.Types.Timeframe.M15 => ChartPeriods.M15,
                Feed.Types.Timeframe.M5 => ChartPeriods.M5,
                Feed.Types.Timeframe.M1 => ChartPeriods.M1,
                Feed.Types.Timeframe.S10 => ChartPeriods.S10,
                Feed.Types.Timeframe.S1 => ChartPeriods.S1,
                Feed.Types.Timeframe.Ticks => ChartPeriods.Ticks,
                _ => throw new ArgumentException("Can't convert provided TimeFrame to ChartPeriod"),
            };
        }

        public static Feed.Types.Timeframe ConvertPeriod(ChartPeriods timeFrame)
        {
            return timeFrame switch
            {
                ChartPeriods.MN1 => Feed.Types.Timeframe.MN,
                ChartPeriods.D1 => Feed.Types.Timeframe.D,
                ChartPeriods.W1 => Feed.Types.Timeframe.W,
                ChartPeriods.H4 => Feed.Types.Timeframe.H4,
                ChartPeriods.H1 => Feed.Types.Timeframe.H1,
                ChartPeriods.M30 => Feed.Types.Timeframe.M30,
                ChartPeriods.M15 => Feed.Types.Timeframe.M15,
                ChartPeriods.M5 => Feed.Types.Timeframe.M5,
                ChartPeriods.M1 => Feed.Types.Timeframe.M1,
                ChartPeriods.S10 => Feed.Types.Timeframe.S10,
                ChartPeriods.S1 => Feed.Types.Timeframe.S1,
                ChartPeriods.Ticks => Feed.Types.Timeframe.Ticks,
                _ => throw new ArgumentException("Can't convert provided ChartPeriod to TimeFrame"),
            };
        }
    }
}

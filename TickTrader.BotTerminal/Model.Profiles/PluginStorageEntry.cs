using System.Runtime.Serialization;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Plugin")]
    internal abstract class PluginStorageEntry<T> where T : PluginStorageEntry<T>, new()
    {
        [DataMember]
        public PluginConfig Config { get; set; }

        public virtual T Clone()
        {
            return new T
            {
                Config = Config,
            };
        }
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Indicator")]
    internal class IndicatorStorageEntry : PluginStorageEntry<IndicatorStorageEntry>
    {
    }


    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "TradeBot")]
    internal class TradeBotStorageEntry : PluginStorageEntry<TradeBotStorageEntry>
    {
        [DataMember]
        public bool Started { get; set; }


        public override TradeBotStorageEntry Clone()
        {
            var res = base.Clone();
            res.Started = Started;
            return res;
        }
    }

    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ColumnState")]
    internal class ColumnStateStorageEntry
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public bool State { get; set; }
    }

    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "ColumnState")]
    internal class ViewModelSettings
    {
        [DataMember]
        public bool HistoryViewSkipCancel { get; set; } = true;

        [DataMember]
        public string HistoryViewPeriod { get; set; }

        [DataMember]
        public string HistoryViewTo { get; set; }

        [DataMember]
        public string HistoryViewFrom { get; set; }
    }
}

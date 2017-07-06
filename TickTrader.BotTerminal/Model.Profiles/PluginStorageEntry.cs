using System.Runtime.Serialization;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Plugin")]
    public abstract class PluginStorageEntry<T> where T : PluginStorageEntry<T>, new()
    {
        [DataMember]
        public string DescriptorId { get; set; }

        [DataMember]
        public string PluginFilePath { get; set; }

        [DataMember]
        public string InstanceId { get; set; }

        [DataMember]
        public bool Isolated { get; set; }

        [DataMember]
        public PluginConfig Config { get; set; }


        public virtual T Clone()
        {
            return new T
            {
                DescriptorId = DescriptorId,
                PluginFilePath = PluginFilePath,
                InstanceId = InstanceId,
                Isolated = Isolated,
                Config = Config,
            };
        }
    }


    [DataContract(Namespace = "", Name = "Indicator")]
    public class IndicatorStorageEntry : PluginStorageEntry<IndicatorStorageEntry>
    {
    }


    [DataContract(Namespace = "", Name = "TradeBot")]
    public class TradeBotStorageEntry : PluginStorageEntry<TradeBotStorageEntry>
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
}

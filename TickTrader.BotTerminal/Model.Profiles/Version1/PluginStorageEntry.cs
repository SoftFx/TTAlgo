using System.Runtime.Serialization;
using TickTrader.Algo.Common.Model.Config.Version1;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal.Model.Profiles.Version1
{
    [DataContract(Namespace = "", Name = "Plugin")]
    internal abstract class PluginStorageEntry<T> where T : PluginStorageEntry<T>, new()
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

        [DataMember]
        public PluginPermissions Permissions { get; set; }


        public virtual T Clone()
        {
            return new T
            {
                DescriptorId = DescriptorId,
                PluginFilePath = PluginFilePath,
                InstanceId = InstanceId,
                Isolated = Isolated,
                Config = Config,
                Permissions = Permissions,
            };
        }
    }


    [DataContract(Namespace = "", Name = "Indicator")]
    internal class IndicatorStorageEntry : PluginStorageEntry<IndicatorStorageEntry>
    {
    }


    [DataContract(Namespace = "", Name = "TradeBot")]
    internal class TradeBotStorageEntry : PluginStorageEntry<TradeBotStorageEntry>
    {
        [DataMember]
        public bool Started { get; set; }

        [DataMember]
        public bool StateViewOpened { get; set; }

        [DataMember]
        public WindowStorageModel StateSettings { get; set; }


        public override TradeBotStorageEntry Clone()
        {
            var res = base.Clone();
            res.Started = Started;
            res.StateViewOpened = StateViewOpened;
            res.StateSettings = StateSettings.Clone();
            return res;
        }
    }
}

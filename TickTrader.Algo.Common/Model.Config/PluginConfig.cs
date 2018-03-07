using System.Collections.Generic;
using System.Runtime.Serialization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Namespace = "TTAlgo.Setup.ver2")]
    [KnownType(typeof(IndicatorConfig))]
    [KnownType(typeof(TradeBotConfig))]
    public class PluginConfig
    {
        [DataMember(Name = "properties")]
        private List<Property> _properties = new List<Property>();


        public List<Property> Properties => _properties;

        [DataMember(Name = "timeframe")]
        public TimeFrames TimeFrame { get; set; }

        [DataMember(Name = "symbol")]
        public string MainSymbol { get; set; }

        [DataMember(Name = "mapping")]
        public string SelectedMapping { get; set; }

        [DataMember(Name = "instanceId")]
        public string InstanceId { get; set; }

        [DataMember(Name = "permissions")]
        public PluginPermissions Permissions { get; set; }
    }


    [DataContract(Namespace = "TTAlgo.Setup.ver2")]
    public class IndicatorConfig : PluginConfig
    {
    }


    [DataContract(Namespace = "TTAlgo.Setup.ver2")]
    public class TradeBotConfig : PluginConfig
    {
    }
}

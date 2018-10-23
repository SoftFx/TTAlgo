using System.Collections.Generic;
using System.Runtime.Serialization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Namespace = "TTAlgo.Setup.ver2")]
    public class PluginConfig
    {
        [DataMember(Name = "Key")]
        public PluginKey Key { get; set; }

        [DataMember(Name = "TimeFrame")]
        public TimeFrames TimeFrame { get; set; }

        [DataMember(Name = "MainSymbol")]
        public SymbolConfig MainSymbol { get; set; }

        [DataMember(Name = "Mapping")]
        public MappingKey SelectedMapping { get; set; }

        [DataMember(Name = "InstanceId")]
        public string InstanceId { get; set; }

        [DataMember(Name = "Permissions")]
        public PluginPermissions Permissions { get; set; }

        [DataMember(Name = "Properties")]
        public List<Property> Properties { get; internal set; }

        public PluginConfig()
        {
            Properties = new List<Property>();
        }
    }
}

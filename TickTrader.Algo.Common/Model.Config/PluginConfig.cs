using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "algoConfig", Namespace = "")]
    [KnownType(typeof(BarBasedConfig))]
    [KnownType(typeof(QuoteBasedConfig))]
    public class PluginConfig
    {
        [DataMember(Name = "properties")]
        private List<Property> properties = new List<Property>();

        public List<Property> Properties => properties;

        [DataMember(Name = "symbol")]
        public string MainSymbol { get; set; }

        [DataMember(Name = "workingfolder")]
        public string WorkingFolder { get;  set; }
    }

    [DataContract(Namespace = "")]
    public class BarBasedConfig : PluginConfig
    {
        [DataMember(Name = "price")]
        public BarPriceType PriceType { get; set; }
    }

    [DataContract(Namespace = "")]
    public class QuoteBasedConfig : PluginConfig
    {
    }
}

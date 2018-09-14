using System.Runtime.Serialization;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "SymbolConfig", Namespace = "TTAlgo.Setup.ver2")]
    public class SymbolConfig
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Origin")]
        public SymbolOrigin Origin { get; set; }
    }


    [DataContract(Name = "Input", Namespace = "TTAlgo.Setup.ver2")]
    public abstract class Input : Property
    {
        [DataMember(Name = "Symbol")]
        public SymbolConfig SelectedSymbol { get; set; }


        public Input()
        {
            SelectedSymbol = new SymbolConfig();
        }
    }

    [DataContract(Name = "QuoteInput", Namespace = "TTAlgo.Setup.ver2")]
    public class QuoteInput : Input
    {
        [DataMember(Name = "Level2")]
        public bool UseL2 { get; set; }
    }

    [DataContract(Name = "MappedInput", Namespace = "TTAlgo.Setup.ver2")]
    public abstract class MappedInput : Input
    {
        [DataMember(Name = "Mapping")]
        public MappingKey SelectedMapping { get; set; }
    }


    [DataContract(Name = "BarToBarInput", Namespace = "TTAlgo.Setup.ver2")]
    public class BarToBarInput : MappedInput
    {
    }


    [DataContract(Name = "BarToDoubleInput", Namespace = "TTAlgo.Setup.ver2")]
    public class BarToDoubleInput : MappedInput
    {
    }


    [DataContract(Name = "QuoteToBarInput", Namespace = "TTAlgo.Setup.ver2")]
    public class QuoteToBarInput : MappedInput
    {
    }

    [DataContract(Name = "QuoteToDoubleInput", Namespace = "TTAlgo.Setup.ver2")]
    public class QuoteToDoubleInput : MappedInput
    {
    }
}

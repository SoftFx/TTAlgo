using System.Runtime.Serialization;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "SymbolConfig", Namespace = "TTAlgo.Config.v2")]
    public class SymbolConfig
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Origin")]
        public SymbolOrigin Origin { get; set; }


        public SymbolConfig Clone()
        {
            return new SymbolConfig
            {
                Name = Name,
                Origin = Origin,
            };
        }
    }


    [DataContract(Name = "Input", Namespace = "TTAlgo.Config.v2")]
    public abstract class Input : Property
    {
        [DataMember(Name = "Symbol")]
        public SymbolConfig SelectedSymbol { get; set; }


        public Input()
        {
            SelectedSymbol = new SymbolConfig();
        }
    }

    [DataContract(Name = "QuoteInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteInput : Input
    {
        [DataMember(Name = "Level2")]
        public bool UseL2 { get; set; }


        public override Property Clone()
        {
            return new QuoteInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), UseL2 = UseL2 };
        }
    }

    [DataContract(Name = "MappedInput", Namespace = "TTAlgo.Config.v2")]
    public abstract class MappedInput : Input
    {
        [DataMember(Name = "Mapping")]
        public MappingKey SelectedMapping { get; set; }
    }


    [DataContract(Name = "BarToBarInput", Namespace = "TTAlgo.Config.v2")]
    public class BarToBarInput : MappedInput
    {
        public override Property Clone()
        {
            return new BarToBarInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }


    [DataContract(Name = "BarToDoubleInput", Namespace = "TTAlgo.Config.v2")]
    public class BarToDoubleInput : MappedInput
    {
        public override Property Clone()
        {
            return new BarToDoubleInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }


    [DataContract(Name = "QuoteToBarInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteToBarInput : MappedInput
    {
        public override Property Clone()
        {
            return new QuoteToBarInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }

    [DataContract(Name = "QuoteToDoubleInput", Namespace = "TTAlgo.Config.v2")]
    public class QuoteToDoubleInput : MappedInput
    {
        public override Property Clone()
        {
            return new QuoteToDoubleInput { Id = Id, SelectedSymbol = SelectedSymbol.Clone(), SelectedMapping = SelectedMapping.Clone() };
        }
    }
}

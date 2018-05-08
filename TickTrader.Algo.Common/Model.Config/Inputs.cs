using System.Runtime.Serialization;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "Input", Namespace = "TTAlgo.Setup.ver2")]
    public abstract class Input : Property
    {
        [DataMember]
        public string SelectedSymbol { get; set; }
    }

    [DataContract(Name = "QuoteInput", Namespace = "TTAlgo.Setup.ver2")]
    public class QuoteInput : Input
    {
        [DataMember]
        public bool UseL2 { get; set; }
    }

    [DataContract(Name = "MappedInput", Namespace = "TTAlgo.Setup.ver2")]
    public abstract class MappedInput : Input
    {
        [DataMember]
        public string SelectedMapping { get; set; }
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

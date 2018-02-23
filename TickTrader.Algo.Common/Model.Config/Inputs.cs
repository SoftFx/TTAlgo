using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "Input", Namespace = "")]
    public abstract class Input : Property
    {
        [DataMember]
        public string SelectedSymbol { get; set; }
    }

    [DataContract(Name = "QuoteInput", Namespace = "")]
    public class QuoteInput : Input
    {
        [DataMember]
        public bool UseL2 { get; set; }
    }

    [DataContract(Name = "MappedInput", Namespace = "")]
    public abstract class MappedInput : Input
    {
        [DataMember]
        public string SelectedMapping { get; set; }
    }


    [DataContract(Name = "BarToBarInput", Namespace = "")]
    public class BarToBarInput : MappedInput
    {
    }


    [DataContract(Name = "BarToDoubleInput", Namespace = "")]
    public class BarToDoubleInput : MappedInput
    {
    }


    [DataContract(Name = "QuoteToBarInput", Namespace = "")]
    public class QuoteToBarInput : MappedInput
    {
    }

    [DataContract(Name = "QuoteToDoubleInput", Namespace = "")]
    public class QuoteToDoubleInput : MappedInput
    {
    }
}

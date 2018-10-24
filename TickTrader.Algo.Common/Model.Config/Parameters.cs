using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "property", Namespace = "TTAlgo.Config.v2")]
    [KnownType(typeof(BoolParameter))]
    [KnownType(typeof(IntParameter))]
    [KnownType(typeof(NullableIntParameter))]
    [KnownType(typeof(DoubleParameter))]
    [KnownType(typeof(NullableDoubleParameter))]
    [KnownType(typeof(StringParameter))]
    [KnownType(typeof(EnumParameter))]
    [KnownType(typeof(FileParameter))]
    [KnownType(typeof(ColoredLineOutput))]
    [KnownType(typeof(MarkerSeriesOutput))]
    [KnownType(typeof(QuoteInput))]
    [KnownType(typeof(QuoteToDoubleInput))]
    [KnownType(typeof(BarToBarInput))]
    [KnownType(typeof(BarToDoubleInput))]
    [KnownType(typeof(QuoteToBarInput))]
    public abstract class Property
    {
        [DataMember(Name = "Key")]
        public string Id { get; set; }
    }

    [DataContract(Name = "Parameter", Namespace = "TTAlgo.Config.v2")]
    public abstract class Parameter : Property
    {
        public abstract object ValObj { get; }
    }

    [DataContract(Name = "Bool", Namespace = "TTAlgo.Config.v2")]
    public class BoolParameter : Parameter<bool>
    {
    }

    [DataContract(Name = "Int", Namespace = "TTAlgo.Config.v2")]
    public class IntParameter : Parameter<int>
    {
    }

    [DataContract(Name = "NullInt", Namespace = "TTAlgo.Config.v2")]
    public class NullableIntParameter : Parameter<int?>
    {
    }

    [DataContract(Name = "Double", Namespace = "TTAlgo.Config.v2")]
    public class DoubleParameter : Parameter<double>
    {
    }

    [DataContract(Name = "NullDouble", Namespace = "TTAlgo.Config.v2")]
    public class NullableDoubleParameter : Parameter<double?>
    {
    }

    [DataContract(Name = "String", Namespace = "TTAlgo.Config.v2")]
    public class StringParameter : Parameter<string>
    {
    }

    [DataContract(Name = "Enum", Namespace = "TTAlgo.Config.v2")]
    public class EnumParameter : Parameter<string>
    {
    }

    [DataContract(Name = "File", Namespace = "TTAlgo.Config.v2")]
    public class FileParameter : Parameter
    {
        [DataMember(Name = "FileName")]
        public string FileName { get; set; }

        public override object ValObj => FileName;
    }

    [DataContract(Namespace = "TTAlgo.Config.v2")]
    public class Parameter<T> : Parameter
    {
        [DataMember(Name = "Value")]
        public T Value { get; set; }

        public override object ValObj => Value;
    }
}

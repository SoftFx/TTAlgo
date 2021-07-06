using System.Runtime.Serialization;

namespace TickTrader.Algo.Core.Config
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
    [KnownType(typeof(BarToBarInput))]
    [KnownType(typeof(BarToDoubleInput))]
    public abstract class Property
    {
        [DataMember(Name = "Key")]
        public string Id { get; set; }


        public abstract Property Clone();
    }

    [DataContract(Name = "Parameter", Namespace = "TTAlgo.Config.v2")]
    public abstract class Parameter : Property
    {
        public abstract object ValObj { get; }
    }

    [DataContract(Name = "Bool", Namespace = "TTAlgo.Config.v2")]
    public class BoolParameter : Parameter<bool>
    {
        public override Property Clone()
        {
            return new BoolParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "Int", Namespace = "TTAlgo.Config.v2")]
    public class IntParameter : Parameter<int>
    {
        public override Property Clone()
        {
            return new IntParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "NullInt", Namespace = "TTAlgo.Config.v2")]
    public class NullableIntParameter : Parameter<int?>
    {
        public override Property Clone()
        {
            return new NullableIntParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "Double", Namespace = "TTAlgo.Config.v2")]
    public class DoubleParameter : Parameter<double>
    {
        public override Property Clone()
        {
            return new DoubleParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "NullDouble", Namespace = "TTAlgo.Config.v2")]
    public class NullableDoubleParameter : Parameter<double?>
    {
        public override Property Clone()
        {
            return new NullableDoubleParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "String", Namespace = "TTAlgo.Config.v2")]
    public class StringParameter : Parameter<string>
    {
        public override Property Clone()
        {
            return new StringParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "Enum", Namespace = "TTAlgo.Config.v2")]
    public class EnumParameter : Parameter<string>
    {
        public override Property Clone()
        {
            return new EnumParameter { Id = Id, Value = Value };
        }
    }

    [DataContract(Name = "File", Namespace = "TTAlgo.Config.v2")]
    public class FileParameter : Parameter
    {
        [DataMember(Name = "FileName")]
        public string FileName { get; set; }

        public override object ValObj => FileName;


        public override Property Clone()
        {
            return new FileParameter { Id = Id, FileName = FileName };
        }
    }

    [DataContract(Namespace = "TTAlgo.Config.v2")]
    public abstract class Parameter<T> : Parameter
    {
        [DataMember(Name = "Value")]
        public T Value { get; set; }

        public override object ValObj => Value;
    }
}

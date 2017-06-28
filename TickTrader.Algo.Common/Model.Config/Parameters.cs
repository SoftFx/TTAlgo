using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Model.Config
{
    [DataContract(Name = "property", Namespace = "")]
    [KnownType(typeof(IntParameter))]
    [KnownType(typeof(DoubleParameter))]
    [KnownType(typeof(StringParameter))]
    [KnownType(typeof(EnumParameter))]
    [KnownType(typeof(FileParameter))]
    public abstract class Property
    {
        [DataMember(Name = "key")]
        public string Id { get; set; }
    }

    [DataContract(Name = "Parameter", Namespace = "")]
    public abstract class Parameter : Property
    {
        public abstract object ValObj { get; }
    }

    [DataContract(Name = "int", Namespace = "")]
    public class IntParameter : Parameter<int>
    {
    }

    [DataContract(Name = "double", Namespace = "")]
    public class DoubleParameter : Parameter<double>
    {
    }

    [DataContract(Name = "string", Namespace = "")]
    public class StringParameter : Parameter<string>
    {
    }

    [DataContract(Name = "enum", Namespace = "")]
    public class EnumParameter : Parameter<string>
    {
    }

    [DataContract(Name = "file", Namespace = "")]
    public class FileParameter : Parameter
    {
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        public override object ValObj => FileName;
    }

    [DataContract(Namespace = "")]
    public class Parameter<T> : Parameter
    {
        [DataMember(Name = "value")]
        public T Value { get; set; }

        public override object ValObj => Value;
    }
}

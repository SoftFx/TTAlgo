using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public enum AlgoPropertyTypes { Unknown, Parameter, InputSeries, OutputSeries }
    public enum AlgoPropertyErrors { SetIsNotPublic, GetIsNotPublic, MultipleAttributes, OutputIsOnlyForIndicators, InputIsOnlyForIndicators }

    internal class AlgoPropertyDescriptor
    {
        public AlgoPropertyDescriptor(AlgoMetadata classMetadata, PropertyInfo reflectioInfo, AlgoPropertyErrors? error = null)
        {
            this.ClassMetadata = classMetadata;
            this.Info = reflectioInfo;
            this.Error = error;
        }

        public AlgoMetadata ClassMetadata { get; private set; }
        public PropertyInfo Info { get; private set; }
        public virtual AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Unknown; } }
        public AlgoPropertyErrors? Error { get; protected set; }
        public bool IsValid { get { return Error == null; } }

        protected virtual AlgoPropertyErrors? Validate()
        {
            if (!Info.SetMethod.IsPublic)
                return AlgoPropertyErrors.SetIsNotPublic;
            else if (!Info.GetMethod.IsPublic)
                return AlgoPropertyErrors.GetIsNotPublic;
            return null;
        }
    }

    internal class ParameterDescriptor : AlgoPropertyDescriptor
    {
        public ParameterDescriptor(AlgoMetadata classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (ParameterAttribute)attribute;
            Error = Validate();
        }

        public ParameterAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }
    }

    internal class InputDescriptor : AlgoPropertyDescriptor
    {
        public InputDescriptor(AlgoMetadata classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (InputAttribute)attribute;
            Error = Validate();
        }

        public InputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.InputSeries; } }
    }

    internal class OutputDescriptor : AlgoPropertyDescriptor
    {
        public OutputDescriptor(AlgoMetadata classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (OutputAttribute)attribute;
            Error = Validate();
        }

        public OutputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.OutputSeries; } }
    }
}

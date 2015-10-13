using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    public enum AlgoPropertyTypes { Unknown, Parameter, InputSeries, OutputSeries }
    public enum AlgoPropertyErrors { SetIsNotPublic, GetIsNotPublic, MultipleAttributes, OutputIsOnlyForIndicators, InputIsOnlyForIndicators }

    internal class AlgoPropertyDescriptor
    {
        public AlgoPropertyDescriptor(AlgoDescriptor classMetadata, PropertyInfo reflectioInfo, AlgoPropertyErrors? error = null)
        {
            this.ClassMetadata = classMetadata;
            this.Info = reflectioInfo;
            this.Error = error;
        }

        public string Id { get { return Info.Name; } }
        public AlgoDescriptor ClassMetadata { get; private set; }
        public PropertyInfo Info { get; private set; }
        public virtual AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Unknown; } }
        public AlgoPropertyErrors? Error { get; protected set; }
        public bool IsValid { get { return Error == null; } }

        public virtual AlgoPropertyInfo GetInteropCopy()
        {
            AlgoPropertyInfo info = new AlgoPropertyInfo();
            FillCommonProperties(info);
            return info;
        }

        protected void FillCommonProperties(AlgoPropertyInfo info)
        {
            info.Id = this.Id;
            info.DisplayName = this.Info.Name;
            info.PropertyType = this.PropertyType;
            info.Error = this.Error;
        }

        protected virtual AlgoPropertyErrors? Validate()
        {
            if (!Info.SetMethod.IsPublic)
                return AlgoPropertyErrors.SetIsNotPublic;
            else if (!Info.GetMethod.IsPublic)
                return AlgoPropertyErrors.GetIsNotPublic;
            return null;
        }

        internal void Set(Api.Algo instance, object value)
        {
            Info.SetValue(instance, value);
        }
    }

    internal class ParameterDescriptor : AlgoPropertyDescriptor
    {
        public ParameterDescriptor(AlgoDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (ParameterAttribute)attribute;
            Error = Validate();
        }

        public override AlgoPropertyInfo GetInteropCopy()
        {
            ParameterInfo copy = new ParameterInfo();
            FillCommonProperties(copy);
            return copy;
        }

        public ParameterAttribute Attribute { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }
    }

    internal class InputDescriptor : AlgoPropertyDescriptor
    {
        public InputDescriptor(AlgoDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (InputAttribute)attribute;
            Error = Validate();
        }

        public override AlgoPropertyInfo GetInteropCopy()
        {
            InputInfo copy = new InputInfo();
            FillCommonProperties(copy);
            return copy;
        }

        public InputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.InputSeries; } }
    }

    internal class OutputDescriptor : AlgoPropertyDescriptor
    {
        public OutputDescriptor(AlgoDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (OutputAttribute)attribute;
            Error = Validate();
        }

        public override AlgoPropertyInfo GetInteropCopy()
        {
            OutputInfo copy = new OutputInfo();
            FillCommonProperties(copy);
            return copy;
        }

        public OutputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.OutputSeries; } }
    }
}

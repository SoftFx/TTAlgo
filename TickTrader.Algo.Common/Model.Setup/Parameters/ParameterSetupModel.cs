using System;
using System.Reflection;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class ParameterSetupModel : PropertySetupModel
    {
        public static readonly string NullableIntTypeName = typeof(int?).GetTypeInfo().FullName;
        public static readonly string NullableDoubleTypeName = typeof(double?).GetTypeInfo().FullName;

        public ParameterMetadata Descriptor { get; }

        public string DataType { get; }

        public bool IsRequired { get; }

        public virtual bool IsReadonly => false;

        public abstract string ValueAsText { get; }

        public string GetQuotedValue()
        {
            var str = ValueAsText;
            if (str.Contains(" "))
                return '"' + ValueAsText + '"';
            return str;
        }

        public ParameterSetupModel(ParameterMetadata descriptor)
        {
            Descriptor = descriptor;
            DataType = descriptor.Descriptor.DataType;
            IsRequired = descriptor.Descriptor.IsRequired;

            SetMetadata(descriptor);
        }

        public abstract object GetValueToApply();

        public override void Apply(IPluginSetupTarget target)
        {
            target.SetParameter(Id, GetValueToApply());
        }

        public class Invalid : ParameterSetupModel
        {
            public override bool IsReadonly => true;

            public override string ValueAsText => "n/a";

            public Invalid(ParameterMetadata descriptor, object error = null)
                : base(descriptor)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
            }

            public override object GetValueToApply()
            {
                return null;
            }

            public override void Load(Property srcProperty)
            {
            }

            public override Property Save()
            {
                throw new Exception("Invalid parameter cannot be saved!");
            }

            public override void Reset()
            {
            }
        }
    }
}

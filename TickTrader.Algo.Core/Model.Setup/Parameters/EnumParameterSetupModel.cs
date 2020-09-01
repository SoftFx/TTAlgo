using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class EnumParamSetupModel : ParameterSetupModel
    {
        public string DefaultValue { get; }

        public List<string> EnumValues { get; }

        public string SelectedValue { get; protected set; }

        public override string ValueAsText => SelectedValue;

        public EnumParamSetupModel(ParameterMetadata descriptor)
            : base(descriptor)
        {
            EnumValues = descriptor.Descriptor.EnumValues;
            if (descriptor.DefaultValue != null && descriptor.DefaultValue is string)
                DefaultValue = (string)descriptor.DefaultValue;
            if (DefaultValue == null)
                DefaultValue = descriptor.Descriptor.EnumValues.FirstOrDefault();
        }


        public override string ToString()
        {
            return $"{DisplayName}: {SelectedValue}";
        }

        public override object GetValueToApply()
        {
            return SelectedValue;
        }

        public override void Reset()
        {
            SelectedValue = DefaultValue;
        }

        public override void Load(Property srcProperty)
        {
            var typedSrcProperty = srcProperty as EnumParameter;
            if (typedSrcProperty != null)
            {
                if (EnumValues.Contains(typedSrcProperty.Value))
                    SelectedValue = typedSrcProperty.Value;
            }
        }

        public override Property Save()
        {
            return new EnumParameter
            {
                Id = Id,
                Value = SelectedValue,
            };
        }
    }
}

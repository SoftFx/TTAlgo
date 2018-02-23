using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class EnumParamSetupModel : ParameterSetupModel
    {
        private string _selectedValue;


        public string DefaultValue { get; }

        public List<string> EnumValues { get; }

        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (_selectedValue == value)
                    return;

                _selectedValue = value;

                NotifyPropertyChanged(nameof(SelectedValue));
            }
        }


        public EnumParamSetupModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
            EnumValues = descriptor.EnumValues;
            if (descriptor.DefaultValue != null && descriptor.DefaultValue is string)
                DefaultValue = (string)descriptor.DefaultValue;
            if (DefaultValue == null)
                DefaultValue = descriptor.EnumValues.FirstOrDefault();
        }


        public override string ToString()
        {
            return $"{DisplayName}: {SelectedValue}";
        }

        public override object GetValueToApply()
        {
            return _selectedValue;
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

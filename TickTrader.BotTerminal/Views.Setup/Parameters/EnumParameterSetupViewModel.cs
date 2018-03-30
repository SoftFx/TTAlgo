using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class EnumParamSetupViewModel : ParameterSetupViewModel
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

                NotifyOfPropertyChange(nameof(SelectedValue));
            }
        }


        public EnumParamSetupViewModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
            EnumValues = metadata.EnumValues;
            if (metadata.DefaultValue != null && metadata.DefaultValue is string)
                DefaultValue = (string)metadata.DefaultValue;
            if (DefaultValue == null)
                DefaultValue = metadata.EnumValues.FirstOrDefault();
        }


        public override string ToString()
        {
            return $"{DisplayName}: {SelectedValue}";
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

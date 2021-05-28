using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

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


        public EnumParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
            EnumValues = descriptor.EnumValues.ToList();
            if (descriptor.DefaultValue != null && descriptor.DefaultValue is string)
                DefaultValue = (string)descriptor.DefaultValue;
            if (DefaultValue == null)
                DefaultValue = descriptor.EnumValues.FirstOrDefault();
        }


        public override string ToString()
        {
            return $"{DisplayName}: {SelectedValue}";
        }

        public override void Reset()
        {
            SelectedValue = DefaultValue;
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var typedSrcProperty = srcProperty as EnumParameterConfig;
            if (typedSrcProperty != null)
            {
                if (EnumValues.Contains(typedSrcProperty.Value))
                    SelectedValue = typedSrcProperty.Value;
            }
        }

        public override IPropertyConfig Save()
        {
            return new EnumParameterConfig
            {
                PropertyId = Id,
                Value = SelectedValue,
            };
        }
    }
}

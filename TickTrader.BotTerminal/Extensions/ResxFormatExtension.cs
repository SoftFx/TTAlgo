using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TickTrader.BotTerminal
{
    class ResxFormatExtension : MarkupExtension, IMultiValueConverter
    {
        public string Key { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Collection<IResxParam> Params { get; private set; }

        public ResxFormatExtension()
        {
            Params = new Collection<IResxParam>();
        }

        public ResxFormatExtension(string resxKey) : this()
        {
            Key = resxKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            MultiBinding formatBinding = new MultiBinding();
            formatBinding.Mode = BindingMode.OneWay;
            formatBinding.ConverterParameter = Key;
            formatBinding.Converter = this;

            formatBinding.Bindings.Add(new Binding(nameof(ResxCore.CurrentCulture))
            {
                Source = ResxCore.Instance,
                Mode = BindingMode.OneWay
            });

            foreach (var param in Params)
            {
                formatBinding.Bindings.Add(param.CreateParamBinding());
            }

            return formatBinding.ProvideValue(serviceProvider);
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            string resxKey = (string)parameter;
            CultureInfo lang = (CultureInfo)values[0];

            object[] formatParams = new object[values.Length - 1];
            for (int i = 1; i < values.Length; i++)
                formatParams[i - 1] = ResolveParameter(values[i]);

            string formatStr = (string)ResxCore.Instance[resxKey];

            return string.Format(formatStr, formatParams);
        }

        private object ResolveParameter(object param)
        {
            return param;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

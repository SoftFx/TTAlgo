using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TickTrader.BotTerminal
{
    public class ResxExtension : MarkupExtension, IValueConverter
    {
        private string _key;

        public ResxExtension(string key)
        {
            _key = key;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ResxCore.Instance[(string)parameter];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var resxBinding = new Binding("CurrentCulture");
            resxBinding.Source = ResxCore.Instance;
            resxBinding.Converter = this;
            resxBinding.ConverterParameter = _key;
            resxBinding.Mode = BindingMode.OneWay;
            return resxBinding.ProvideValue(serviceProvider);
        }
    }
}

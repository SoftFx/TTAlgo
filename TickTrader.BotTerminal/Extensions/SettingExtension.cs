using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public interface ISettings
    {
        object this[string key] { get; set; }
    }


    public static class Settings
    {
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.RegisterAttached(
            "Provider", typeof(ISettings), typeof(Settings),
            new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.Inherits));


        public static void SetProvider(DependencyObject element, ISettings value)
        {
            element.SetValue(ProviderProperty, value);
        }

        public static ISettings GetProvider(DependencyObject element)
        {
            return (ISettings)element.GetValue(ProviderProperty);
        }
    }


    [MarkupExtensionReturnType(typeof(object))]
    public class SettingExtension : MarkupExtension, IValueConverter
    {
        private Logger _logger;
        private string _settingName;


        public object Default { get; set; }


        public SettingExtension(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name parameter must not be empty.");

            _logger = NLog.LogManager.GetCurrentClassLogger();
            _settingName = name;
            Default = DependencyProperty.UnsetValue;
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rexBinding = new Binding()
            {
                FallbackValue = Default,
                Path = new PropertyPath($"(0)[{_settingName}]", Settings.ProviderProperty),
                Mode = BindingMode.TwoWay,
                RelativeSource = RelativeSource.Self
            };

            return rexBinding.ProvideValue(serviceProvider);
        }


        #region IValueConverter implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result;
            if (TryConvert(value, targetType, out result))
                return result;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }


        private bool TryConvert(object value, Type targetType, out object result)
        {
            if (value == null)
            {
                result = null;
                return true;
            }

            if (value.GetType() == targetType)
            {
                result = value;
                return true;
            }

            if (value is Int32 && targetType == typeof(double))
            {
            }

            result = value;
            return false;
        }

        #endregion
    }
}

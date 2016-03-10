using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    [MarkupExtensionReturnType(typeof(object))]
    public class SettingExtension : MarkupExtension, IValueConverter
    {
        private string settingName;

        public SettingExtension(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name parameter must not be empty.");

            this.settingName = name;
            this.Default = DependencyProperty.UnsetValue;
        }

        public object Default { get; set; }

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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget targetService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var containerObject = targetService.TargetObject as DependencyObject;

            ISettings provider = null;

            if (containerObject == null)
                System.Diagnostics.Debug.WriteLine("Settings binding '" + settingName + "' is invalid: Settings extention only works for Dependency Properties.");
            else
            {
                provider = Settings.GetProvider(containerObject);
                if (provider == null)
                    System.Diagnostics.Debug.WriteLine("Settings provider is not configured for property with key '" + settingName + "'. Fill attached property 'Settings.Provider' to specify a provider.");
            }
                
            var resxBinding = new Binding("Value");
            resxBinding.FallbackValue = Default;
            if (provider != null)
                resxBinding.Source = new PropertyProxy(settingName, provider);
            else
                resxBinding.Source = new NullProxy();
            resxBinding.Mode = BindingMode.TwoWay;
            return resxBinding.ProvideValue(serviceProvider);
        }

        internal class NullProxy
        {
            public object Value { get; set; }
        }

        internal class PropertyProxy : ObservableObject
        {
            private string propertyKey;
            private ISettings settingObj;

            public PropertyProxy(string key, ISettings settingObj)
            {
                this.propertyKey = key;
                this.settingObj = settingObj;
            }

            public object Value
            {
                get
                {
                    var storageVal = settingObj.GetProperty(propertyKey);
                    if (storageVal == null)
                        return DependencyProperty.UnsetValue;
                    return storageVal;
                }
                set
                {
                    settingObj.SetProperty(propertyKey, value);
                }
            }
        }
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

    public interface ISettings
    {
        object GetProperty(string key);
        void SetProperty(string key, object value);
    }
}

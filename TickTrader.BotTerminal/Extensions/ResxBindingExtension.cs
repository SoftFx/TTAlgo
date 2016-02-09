using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TickTrader.BotTerminal
{
    public class ResxBindingExtension : MarkupExtension, IMultiValueConverter, IValueConverter, IResxParam
    {
        public ResxBindingExtension()
        {
        }

        public ResxBindingExtension(string path)
        {
            Path = path;
        }

        public string Prefix { get; set; }
        public string Path { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var resxBinding = new MultiBinding();
            resxBinding.ConverterParameter = Prefix;
            resxBinding.Converter = this;
            resxBinding.Bindings.Add(
                new Binding(nameof(ResxCore.CurrentCulture))
                {
                    Source = ResxCore.Instance,
                    Mode = BindingMode.OneWay
                });
            resxBinding.Bindings.Add(CreateKeyBinding());
            return resxBinding.ProvideValue(serviceProvider);
        }

        public Binding CreateParamBinding()
        {
            var binding = CreateKeyBinding();
            binding.Converter = this;
            return binding;
        }

        private Binding CreateKeyBinding()
        {
            return new Binding(Path);
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string resxKey = values[1].ToString();
            if (parameter != null)
                resxKey = (string)parameter + resxKey;
            return ResxCore.Instance[resxKey];
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value.ToString();
            return new KeyToken((string)parameter + value.ToString());
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class KeyToken
    {
        public KeyToken(string key)
        {
            this.ResxKey = key;
        }

        public string ResxKey { get; private set; }
    }

    public class ParamBinding : IResxParam
    {
        public ParamBinding()
        {
        }

        public ParamBinding(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        public Binding CreateParamBinding()
        {
            Binding paramBinding = new Binding(Path);
            paramBinding.Mode = BindingMode.OneWay;
            return paramBinding;
        }
    }

    public interface IResxParam
    {
        Binding CreateParamBinding();
    }
}

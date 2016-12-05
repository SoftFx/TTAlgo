using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class AutoViewManager : PropertyChangedBase
    {
        private string skin;
        private IMultiValueConverter styleConverter = new AutoStyleConverter();

        public string StylePostfix
        {
            get { return skin; }
            set
            {
                skin = value;
                NotifyOfPropertyChange(nameof(StylePostfix));
            }
        }

        public UIElement CreateView(Type modelType, object context)
        {
            if (typeof(IWindowModel).IsAssignableFrom(modelType))
            {
                var view = new AutoWindowView();
                BindViewToStyle(modelType, view, context);
                return view;
            }
            else
            {
                var view = new AutoView();
                BindViewToStyle(modelType, view, context);
                return view;
            }
        }

        private void BindViewToStyle(Type modelType, UIElement view, object context)
        {
            MultiBinding styleBinding = new MultiBinding();
            styleBinding.Converter = styleConverter;
            styleBinding.ConverterParameter = MakeStyleName(modelType, context);
            styleBinding.Bindings.Add(new Binding() { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            styleBinding.Bindings.Add(new Binding("StylePostfix") { Source = this });
            BindingOperations.SetBinding(view, FrameworkElement.StyleProperty, styleBinding);
        }

        private static string MakeStyleName(Type modelType, object context)
        {
            var names = ViewLocator.TransformName(modelType.FullName, context);
            var parts = names.First().Split('.');
            return parts.Last() + "Style";
        }

        public class AutoStyleConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                FrameworkElement targetElement = (FrameworkElement)values[0];
                string styleName = (string)parameter;
                string postfix = (string)values[1];
                string fullStyleName;

                if (string.IsNullOrEmpty(postfix))
                    fullStyleName = styleName;
                else
                    fullStyleName = styleName + "." + postfix;

                return (Style)targetElement.TryFindResource(fullStyleName);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class AutoView : Control { }
    public class AutoWindowView : Window { }
    public interface IWindowModel { }
}


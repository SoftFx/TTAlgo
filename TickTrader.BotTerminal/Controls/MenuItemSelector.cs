using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class MenuItemSelector : MenuItem
    {
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(MenuItemSelector));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public MenuItemSelector()
        {
            AddHandler(MenuItem.ClickEvent, new RoutedEventHandler((s, e) =>
                {
                    e.Handled = true;
                    SelectedItem = ((MenuItem)e.OriginalSource).DataContext;
                }));
        }

        public static readonly IMultiValueConverter Converter = new CheckConverter();

        internal class CheckConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values.Length != 2)
                    throw new InvalidOperationException("IfEqualsConverer accepts exact two values");

                return values[0] != null && values[0].Equals(values[1]);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }   
}

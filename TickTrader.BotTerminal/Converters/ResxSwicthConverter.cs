using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace TickTrader.BotTerminal
{
    [ContentProperty("Cases")]
    public class ResxSwicthConverter : DependencyObject, IValueConverter
    {
        public static DependencyProperty DefaultProperty = DependencyProperty.Register("Default", typeof(object), typeof(ResxSwicthConverter));

        public ResxSwicthConverter()
        {
            Cases = new List<ResxCase>();
        }

        public object Default
        {
            get { return this.GetValue(DefaultProperty); }
            set { this.SetValue(DefaultProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (ResxCase rCase in Cases)
            {
                if (rCase == null && value != null)
                    continue;

                if (value != null && !value.Equals(rCase.Value))
                    continue;

                return rCase.Resx;
            }

            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public List<ResxCase> Cases { get; set; }
    }

    public class ResxCase : DependencyObject
    {
        public static DependencyProperty ResxProperty = DependencyProperty.Register("Resx", typeof(object), typeof(ResxCase));

        public object Value { get; set; }

        public object Resx
        {
            get { return this.GetValue(ResxProperty); }
            set { this.SetValue(ResxProperty, value); }
        }
    }
}

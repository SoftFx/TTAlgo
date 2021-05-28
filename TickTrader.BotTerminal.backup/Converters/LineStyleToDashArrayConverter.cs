using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    [ValueConversion(sourceType: typeof(Metadata.Types.LineStyle), targetType: typeof(double[]))]
    class LineStyleToDashArrayConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new DoubleCollection(value is Metadata.Types.LineStyle ? ((Metadata.Types.LineStyle)value).ToStrokeDashArray() : new double[0]);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

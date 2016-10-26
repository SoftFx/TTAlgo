using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class NumericalComparative : MarkupExtension, IValueConverter
    {
        public enum ComparisonOptions { EqualZero, AboveZero, LessZero, AboveOrEqualZero, LessOrEqualZero }

        public ComparisonOptions ComparisonOption { get; set; }
        public decimal Number { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is int)
                return Compare((int)value, (int)Number);
            if (value is double)
                return Compare((double)value, (double)Number);
            if (value is decimal)
                return Compare((decimal)value, Number);

            throw new ArgumentException("Parameter must be a number", nameof(value));
        }

        private bool Compare<T>(T number1, T number2) where T : struct, IComparable 
        {
            switch (ComparisonOption)
            {
                case ComparisonOptions.EqualZero:
                    return number1.Equals(number2);
                case ComparisonOptions.AboveZero:
                    return number1.CompareTo(number2) > 0;
                case ComparisonOptions.AboveOrEqualZero:
                    return number1.CompareTo(number2) >= 0;
                case ComparisonOptions.LessZero:
                    return number1.CompareTo(number2) < 0;
                case ComparisonOptions.LessOrEqualZero:
                    return number1.CompareTo(number2) <= 0;
                default: return false;
            }
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

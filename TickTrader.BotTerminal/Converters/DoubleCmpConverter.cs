using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    class DoubleCmpConverter : IValueConverter
    {
        public CmpConverterModes Mode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var num1 = System.Convert.ToDouble(value);
            var num2 = System.Convert.ToDouble(parameter);

            switch (Mode)
            {
                case CmpConverterModes.Less: return num1 < num2;
                case CmpConverterModes.LessOrEqual: return num1 <= num2;
                case CmpConverterModes.Greater: return num1 > num2;
                case CmpConverterModes.GreateOrEqual: return num1 >= num2;
            }

            throw new NotImplementedException("Unsupported mode:" + Mode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    enum CmpConverterModes { Less, LessOrEqual, Greater, GreateOrEqual }
}

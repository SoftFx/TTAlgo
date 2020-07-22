using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace TickTrader.BotTerminal
{
    internal sealed class PricePrecisionConverter<T> : IDisplayValueConverter<T>
    {
        private readonly string _priceFormat;

        internal PricePrecisionConverter(int precision)
        {
            //_priceFormat = $"0.{new string('#', precision)}";
            _priceFormat = $"F{precision}";
        }

        public string Convert(T val)
        {
            if (val is double || val is decimal)
                return (val as IFormattable).ToString(_priceFormat, CultureInfo.InvariantCulture);
            else
                return val.ToString();
        }
    }
}

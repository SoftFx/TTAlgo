using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Converters
{
    internal sealed class AmountToLotsConverter<T> : IDisplayValueConverter<T>
    {
        private readonly double _lotSize = 1;

        internal AmountToLotsConverter(double lotSize)
        {
            _lotSize = lotSize;
        }

        public string Convert(T val)
        {
            if (val is double d)
                return (d / _lotSize).ToString(CultureInfo.InvariantCulture);
            else
            if (val is decimal dd)
                return (dd / (decimal)_lotSize).ToString(CultureInfo.InvariantCulture);
            else
                return val.ToString();
        }
    }
}

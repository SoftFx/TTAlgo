using Machinarium.Var;
using System;
using System.Globalization;

namespace TickTrader.BotTerminal.Converters
{
    internal sealed class PricePrecisionConverter<T> : IDisplayValueConverter<T>
    {
        private readonly string _priceFormat;
        private readonly bool _isDoubleConverter;

        internal PricePrecisionConverter(int precision)
        {
            _priceFormat = $"F{precision}";
            _isDoubleConverter = typeof(T) == typeof(double);
        }

        public string Convert(T val) //TODO: redone
        {
            if (_isDoubleConverter)
            {
                var doubleVolume = (double)System.Convert.ChangeType(val, typeof(double));

                return double.IsNaN(doubleVolume) ? string.Empty : doubleVolume.ToString(_priceFormat, CultureInfo.InvariantCulture);
            }
            else
            if (val is double || val is decimal)
                return (val as IFormattable).ToString(_priceFormat, CultureInfo.InvariantCulture);
            else
                return val.ToString();
        }
    }
}

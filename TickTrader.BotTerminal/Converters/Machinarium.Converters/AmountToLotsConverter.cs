using Machinarium.Var;
using System.Globalization;

namespace TickTrader.BotTerminal.Converters
{
    internal sealed class AmountToLotsConverter<T> : IDisplayValueConverter<T>
    {
        private static readonly string DefaultVolumeFormat = $"0.{new string('#', 15)}";

        private readonly double _lotSize = 1;

        internal AmountToLotsConverter(double lotSize)
        {
            _lotSize = lotSize;
        }

        public string Convert(T val)
        {
            if (val is double d)
                return (d / _lotSize).ToString(DefaultVolumeFormat, CultureInfo.InvariantCulture);
            else
            if (val is decimal dd)
                return (dd / (decimal)_lotSize).ToString(DefaultVolumeFormat, CultureInfo.InvariantCulture);
            else
                return val.ToString();
        }
    }
}

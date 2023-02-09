using Machinarium.Var;
using System;

namespace TickTrader.BotTerminal.Converters.Machinarium.Converters
{
    internal sealed class DateTimeToUtc : IDisplayValueConverter<DateTime?>
    {
        public const string FullTimeFormat = "HH:mm:ss.fff dd-MM-yyyy";

        public string Convert(DateTime? val) => val?.ToUniversalTime().ToString(FullTimeFormat) ?? string.Empty;
    }
}

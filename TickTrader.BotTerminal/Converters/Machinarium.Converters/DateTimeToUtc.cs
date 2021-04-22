using Machinarium.Var;
using System;

namespace TickTrader.BotTerminal.Converters.Machinarium.Converters
{
    internal sealed class DateTimeToUtc : IDisplayValueConverter<DateTime?>
    {
        public const string FullTimeFormat = "dd-MMM-yyyy HH:mm:ss.fff";

        public string Convert(DateTime? val) => val?.ToUniversalTime().ToString(FullTimeFormat) ?? string.Empty;
    }
}

using System.Text;
using NLog;
using NLog.LayoutRenderers;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public static class LoggerHelper
    {
        public static string LoggerNamePrefix = $"{nameof(BotJournal)}.";


        public static string GetBotLoggerName(string botName)
        {
            return $"{LoggerNamePrefix}{PathHelper.GetSafeFileName(botName)}";
        }
    }

    public class BotNameLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.LoggerName.StartsWith(LoggerHelper.LoggerNamePrefix))
                builder.Append(logEvent.LoggerName.Substring(LoggerHelper.LoggerNamePrefix.Length));
            else
                builder.Append(logEvent.LoggerName);
        }
    }
}

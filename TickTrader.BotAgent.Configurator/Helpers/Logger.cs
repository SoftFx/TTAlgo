using NLog;
using System;

namespace TickTrader.BotAgent.Configurator
{
    public static class Logger
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Info(string category, string oldVal, string newVal)
        {
            Info($"{category} was changed: {oldVal} to {newVal}");
        }

        public static void Info(string message)
        {
            _logger.Info(message);
        }

        public static void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public static void Fatal(Exception ex)
        {
            _logger.Fatal(ex);
        }
    }
}

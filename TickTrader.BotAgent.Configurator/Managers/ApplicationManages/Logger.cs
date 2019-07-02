using System;

namespace TickTrader.BotAgent.Configurator
{
    public static class Logger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Error(Exception ex, string message = null)
        {
            _logger.Error(ex, message);
        }

        public static void Fatal(Exception ex, string message = null)
        {
            _logger.Fatal(ex, message);
        }
    }
}

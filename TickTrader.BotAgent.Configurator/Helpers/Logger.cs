using NLog;
using System;

namespace TickTrader.BotAgent.Configurator
{
    public static class Logger
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Error(Exception ex)
        {
            _logger.Error(ex);
        }
    }
}

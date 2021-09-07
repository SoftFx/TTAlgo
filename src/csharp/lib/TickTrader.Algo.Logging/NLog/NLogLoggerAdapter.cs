using NLog;
using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Logging
{
    public sealed class NLogLoggerAdapter : IAlgoLogger
    {
        private readonly Logger _logger;


        private NLogLoggerAdapter(string componentName)
        {
            _logger = LogManager.GetLogger(componentName);
        }


        public static IAlgoLogger Create(string componentName) => new NLogLoggerAdapter(componentName);


        public void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            _logger.Debug(msgFormat, msgParams);
        }

        public void Error(string msg)
        {
            _logger.Error(msg);
        }

        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public void Error(Exception ex, string msg)
        {
            _logger.Error(ex, msg);
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            _logger.Error(ex, msgFormat, msgParams);
        }

        public void Info(string msg)
        {
            _logger.Info(msg);
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            _logger.Info(msgFormat, msgParams);
        }
    }
}

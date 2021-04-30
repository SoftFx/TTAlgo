using NLog;
using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.BA
{
    public class LoggerAdapter : IAlgoLogger
    {
        private ILogger _innerLogger;

        public LoggerAdapter(ILogger msLogger)
        {
            _innerLogger = msLogger;
        }

        public void Debug(string msg)
        {
            _innerLogger.Debug(msg);
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            _innerLogger.Debug(msgFormat, msgParams);
        }

        public void Error(string msg)
        {
            _innerLogger.Error(msg);
        }

        public void Error(Exception ex)
        {
            _innerLogger.Error(ex);
        }

        public void Error(Exception ex, string msg)
        {
            _innerLogger.Error(ex, msg);
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            _innerLogger.Error(ex, msgFormat, msgParams);
        }

        public void Info(string msg)
        {
            _innerLogger.Info(msg);
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            _innerLogger.Info(msgFormat, msgParams);
        }
    }
}

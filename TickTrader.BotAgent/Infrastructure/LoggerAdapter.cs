using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.BA
{
    public class LoggerAdapter : IAlgoCoreLogger
    {
        private ILogger _msLogger;

        public LoggerAdapter(ILogger msLogger)
        {
            _msLogger = msLogger;
        }

        public void Debug(string msg)
        {
            _msLogger.LogDebug(msg);
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            _msLogger.LogDebug(msgFormat, msgParams);
        }

        public void Error(string msg)
        {
            _msLogger.LogError(msg);
        }

        public void Error(Exception ex)
        {
            _msLogger.LogError(ex.ToString());
        }

        public void Error(string msg, Exception ex)
        {
            _msLogger.LogError(msg);
            _msLogger.LogError(ex.ToString());
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            _msLogger.LogError(msgFormat, msgParams);
            _msLogger.LogError(ex.ToString());
        }

        public void Info(string msg)
        {
            _msLogger.LogInformation(msg);
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            _msLogger.LogInformation(msgFormat, msgParams);
        }
    }
}

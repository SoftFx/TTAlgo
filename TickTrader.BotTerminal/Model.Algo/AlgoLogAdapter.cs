using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class AlgoLogAdapter : IAlgoCoreLogger
    {
        private readonly Logger logger;

        public AlgoLogAdapter(string componentName)
        {
            logger = NLog.LogManager.GetLogger(componentName);
        }

        public void Debug(string msg)
        {
            logger.Debug(msg);
        }

        public void Debug(string msgFormat, params object[] msgParams)
        {
            logger.Debug(msgFormat, msgParams);
        }

        public void Error(string msg)
        {
            logger.Error(msg);
        }

        public void Error(Exception ex)
        {
            logger.Error(ex);
        }

        public void Error(string msg, Exception ex)
        {
            logger.Error(ex, msg);
        }

        public void Error(Exception ex, string msgFormat, params object[] msgParams)
        {
            logger.Error(ex, msgFormat, msgParams);
        }

        public void Info(string msg)
        {
            logger.Info(msg);
        }

        public void Info(string msgFormat, params object[] msgParams)
        {
            logger.Info(msgFormat, msgParams);
        }
    }
}

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

        public void Error(string msg, Exception ex)
        {
            logger.Error(ex, msg);
        }

        public void Info(string msg)
        {
            logger.Info(msg);
        }
    }
}

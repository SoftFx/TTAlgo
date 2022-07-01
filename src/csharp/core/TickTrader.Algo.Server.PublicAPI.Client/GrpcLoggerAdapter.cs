#if NETFRAMEWORK
using System;
using Grpc.Core.Logging;

namespace TickTrader.Algo.Server.Common.Grpc
{
    public class GrpcLoggerAdapter : ILogger
    {
        private NLog.ILogger _logger;


        public GrpcLoggerAdapter(NLog.ILogger logger)
        {
            _logger = logger;
        }


        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            _logger.Debug(format, formatArgs);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            _logger.Error(format, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            _logger.Info(format, formatArgs);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            _logger.Warn(format, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            _logger.Warn(exception, message);
        }
    }
}
#endif

using Google.Protobuf;
using NLog;

namespace TickTrader.Algo.Protocol.Grpc
{
    internal class MessageFormatter
    {
        private JsonFormatter _formatter;


        public bool LogMessages { get; set; }


        public MessageFormatter()
        {
            _formatter = new JsonFormatter(new JsonFormatter.Settings(true));
        }


        public string ToJson(IMessage message)
        {
            return _formatter.Format(message);
        }

        public void LogClientRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"client > {request.GetType().Name}: {_formatter.Format(request)}");
            }
        }

        public void LogClientResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"client < {request.GetType().Name}: {_formatter.Format(request)}");
            }
        }

        public void LogServerRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"server < {request.GetType().Name}: {_formatter.Format(request)}");
            }
        }

        public void LogServerResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"server > {request.GetType().Name}: {_formatter.Format(request)}");
            }
        }
    }
}

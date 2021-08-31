using Google.Protobuf;
using Google.Protobuf.Reflection;
using NLog;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class ApiMessageFormatter : MessageFormatter
    {
        public ApiMessageFormatter(FileDescriptor serviceDescriptor) : base(serviceDescriptor) { }


        public void LogClientUpdate(ILogger logger, IMessage update)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client < {update.Descriptor.Name}: {Format(update)}");
            }
        }

        public void LogHeartbeat(ILogger logger)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client < {HeartbeatUpdate.Descriptor.Name}");
            }
        }
    }
}

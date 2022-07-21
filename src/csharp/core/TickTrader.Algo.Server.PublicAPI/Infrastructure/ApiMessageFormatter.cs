using Google.Protobuf;
using Google.Protobuf.Reflection;
using NLog;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class ApiMessageFormatter : MessageFormatter
    {
        public ApiMessageFormatter(FileDescriptor serviceDescriptor) : base(serviceDescriptor) { }


        public void LogUpdateFromServer(ILogger logger, IMessage update, int packedSize, bool compressed)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"server > {update.Descriptor.Name}({packedSize} bytes{(compressed ? ", compressed" : "")}): {Format(update)}");
            }
        }

        public void LogHeartbeat(ILogger logger)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"server < {HeartbeatUpdate.Descriptor.Name}");
            }
        }
    }
}

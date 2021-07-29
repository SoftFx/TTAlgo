using Google.Protobuf.Reflection;
using NLog;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class ApiMessageFormatter : MessageFormatter
    {
        public ApiMessageFormatter(FileDescriptor serviceDescriptor) : base(serviceDescriptor) { }


        public void LogClientUpdate(ILogger logger, IUpdateInfo updateInfo)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client < {updateInfo.ValueMsg.Descriptor.Name}: {{ Value = {Format(updateInfo.ValueMsg)} }}");
            }
        }
    }
}

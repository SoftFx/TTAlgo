#if NETFRAMEWORK
using Grpc.Core;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.Common.Grpc;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal class GrpcChannelProxy : IGrpcChannelProxy
    {
        private readonly Channel _channel;


        public bool IsShutdownState => _channel == null || _channel.State == ChannelState.Shutdown;


        public GrpcChannelProxy(IClientSessionSettings settings, NLog.ILogger logger)
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(logger));

            var address = settings.ServerAddress;
            if (address == "localhost-h2c")
            {
                _channel = new Channel("localhost", settings.ServerPort, ChannelCredentials.Insecure);
            }
            else
            {
                var creds = new SslCredentials(CertificateProvider.RootCertificate);
                var options = new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, "bot-agent.soft-fx.lv"), };

                _channel = new Channel(address, settings.ServerPort, creds, options);
            }
        }


        public CallInvoker GetCallInvoker() => _channel.CreateCallInvoker();

        public Task ConnectAsync(DateTime deadline) => _channel.ConnectAsync(deadline);

        public Task ShutdownAsync() => _channel.ShutdownAsync();

        public void Dispose() { }
    }
}
#endif

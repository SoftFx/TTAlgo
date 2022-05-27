#if NET5_0_OR_GREATER
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal class GrpcChannelProxy : IGrpcChannelProxy
    {
        private readonly GrpcChannel _channel;
        private readonly X509Certificate2 _algoRootCert;


        public bool IsShutdownState => _channel == null || _channel.State == ConnectivityState.Shutdown;


        public GrpcChannelProxy(IClientSessionSettings settings, NLog.ILogger logger)
        {
            _algoRootCert = X509Certificate2.CreateFromPem(CertificateProvider.RootCertificate);

            var handler = new SocketsHttpHandler();
            handler.SslOptions.RemoteCertificateValidationCallback = ServerCertValidationCallback;

            var options = new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.SecureSsl,
                HttpHandler = handler,
            };

            _channel = GrpcChannel.ForAddress($"https://{settings.ServerAddress}:{settings.ServerPort}", options);
        }


        public CallInvoker GetCallInvoker() => _channel.CreateCallInvoker();

        public Task ConnectAsync(DateTime deadline)
        {
            var cancelTokenSrc = new CancellationTokenSource(deadline - DateTime.UtcNow);
            return _channel.ConnectAsync(cancelTokenSrc.Token);
        }

        public Task ShutdownAsync() => _channel.ShutdownAsync();

        public void Dispose() => _channel.Dispose();


        private bool ServerCertValidationCallback(object sender, X509Certificate serverCert, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;

            if (_algoRootCert != null && errors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {
                chain.ChainPolicy.CustomTrustStore.Clear();
                chain.ChainPolicy.CustomTrustStore.Add(_algoRootCert);
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                return chain.Build((X509Certificate2)serverCert);
            }

            return false;
        }
    }
}
#endif

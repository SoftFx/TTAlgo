#if NET5_0_OR_GREATER
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal class GrpcChannelProxy : IGrpcChannelProxy
    {
        private readonly NLog.ILogger _logger;
        private readonly GrpcChannel _channel;
        private readonly X509Certificate2 _algoRootCert;


        public bool IsShutdownState => _channel == null || _channel.State == ConnectivityState.Shutdown;


        public GrpcChannelProxy(IClientSessionSettings settings, NLog.ILogger logger)
        {
            _algoRootCert = X509Certificate2.CreateFromPem(CertificateProvider.RootCertificate);
            _logger = logger;

            var handler = new SocketsHttpHandler();
            handler.SslOptions.RemoteCertificateValidationCallback = ServerCertValidationCallback;

            var address = settings.ServerAddress;
            if (address == "localhost-h2c")
            {
                var options = new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Insecure,
                    HttpHandler = handler,
                };

                _channel = GrpcChannel.ForAddress($"http://localhost:{settings.ServerPort}", options);
            }
            else
            {
                var options = new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.SecureSsl,
                    HttpHandler = handler,
                };

                _channel = GrpcChannel.ForAddress($"https://{address}:{settings.ServerPort}", options);
            }
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

            var hasChainErrors = errors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors);
            // SslPolicyErrors.RemoteCertificateChainErrors is not set when running under Wine
            var hasChainStatusErrors = chain.ChainStatus.Length > 0;
            if (_algoRootCert != null && (hasChainErrors || hasChainStatusErrors))
            {
                chain.ChainPolicy.CustomTrustStore.Clear();
                chain.ChainPolicy.CustomTrustStore.Add(_algoRootCert);
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                var res = chain.Build((X509Certificate2)serverCert);
                if (!res)
                    LogCertValidationErrors("CustomRootTrust", errors, chain);
                return res;
            }

            LogCertValidationErrors("Fallback", errors, chain);
            return false;
        }

        private void LogCertValidationErrors(string marker, SslPolicyErrors errors, X509Chain chain)
        {
            var sb = new StringBuilder();
            sb.Append($"X509 validation failed({marker}): errors={errors}");
            for (var i = 0; i < chain.ChainStatus.Length; i++)
            {
                var status = chain.ChainStatus[i];
                sb.AppendLine();
                sb.Append($"ChainStatus[{i}]={status.Status}: {status.StatusInformation}");
            }
            _logger.Debug(sb.ToString());
        }
    }
}
#endif

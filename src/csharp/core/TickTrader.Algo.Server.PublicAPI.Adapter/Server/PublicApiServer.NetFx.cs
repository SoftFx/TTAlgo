#if NETFRAMEWORK
using Grpc.Core;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;
using GrpcCore = Grpc.Core;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public enum ServerStates { Started, Stopped, Faulted }

    public class ServerSettings
    {
        public int ServerPort { get; }

        public string LogDirectory { get; }

        public bool LogMessages { get; }


        public ServerSettings(int serverPort, string logDirectory, bool logMessages)
        {
            ServerPort = serverPort;
            LogDirectory = logDirectory;
            LogMessages = logMessages;
        }
    }

    public class PublicApiServer
    {
        private readonly ILogger _logger;
        private readonly AlgoServerPublicImpl _impl;
        private readonly VersionSpec _version;
        private readonly ServerSettings _settings;

        private GrpcCore.Server _server;


        public ServerStates State { get; private set; }


        public PublicApiServer(IAlgoServerApi server, IAuthManager authManager, IJwtProvider jwtProvider, ServerSettings settings)
        {
            _logger = LoggerHelper.GetLogger(nameof(PublicApiServer), settings.LogDirectory, GetType().Name);
            _version = new VersionSpec();
            _impl = new AlgoServerPublicImpl(server, authManager, jwtProvider, _logger, settings.LogMessages, _version);
        }


        public Task Start()
        {
            try
            {
                if (State != ServerStates.Stopped)
                    throw new Exception($"Server is already {State}");

                _impl.Start();
                var creds = new SslServerCredentials(new[] { new KeyCertificatePair(CertificateProvider.ServerCertificate, CertificateProvider.ServerKey), });
                _server = new GrpcCore.Server
                {
                    Services = { AlgoServerPublic.BindService(_impl) },
                    Ports = { new ServerPort("0.0.0.0", _settings.ServerPort, creds) },
                };
                _server.Start();
                State = ServerStates.Started;

                _logger.Info("Server started");
                _logger.Info($"Server current version: {_version.CurrentVersionStr}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start public api server: {ex.Message}");
                State = ServerStates.Faulted;
            }

            return Task.CompletedTask;
        }

        public async Task Shutdown()
        {
            try
            {
                if (State != ServerStates.Started)
                    return;

                await _impl.Shutdown();
                await _server.ShutdownAsync();
                State = ServerStates.Stopped;

                _logger.Info("Server stopped");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to stop public api server: {ex.Message}");
                State = ServerStates.Faulted;
            }
        }
    }
}
#endif

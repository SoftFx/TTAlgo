using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public enum ServerStates { Stopped, Started, Faulted }

    public record ServerSettings(string LogDirectory, bool LogMessages);

    public class PublicApiServer
    {
        private readonly ILogger _logger;
        private readonly VersionSpec _version;


        public ServerStates State { get; private set; }

        public AlgoServerPublicImpl Impl { get; }


        public PublicApiServer(IAlgoServerApi server, IAuthManager authManager, IJwtProvider jwtProvider, ServerSettings settings)
        {
            _logger = LoggerHelper.GetLogger(nameof(PublicApiServer), settings.LogDirectory, GetType().Name);
            _version = new VersionSpec();
            Impl = new AlgoServerPublicImpl(server, authManager, jwtProvider, _logger, settings.LogMessages, _version);
            State = ServerStates.Stopped;
        }


        public Task Start()
        {
            try
            {
                if (State != ServerStates.Stopped)
                    throw new Exception($"Server is already {State}");

                Impl.Start();
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

                await Impl.Shutdown();
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

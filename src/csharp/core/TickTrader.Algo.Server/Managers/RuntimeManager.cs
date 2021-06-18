using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class RuntimeManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly IActorRef _impl;


        public RuntimeManager(AlgoServer server)
        {
            _impl = ActorSystem.SpawnLocal<Impl>(nameof(RuntimeManager), new InitMsg(server));
        }


        public Task<PkgRuntimeModel> GetPkgRuntime(string pkgId) => _impl.Ask<PkgRuntimeModel>(new PkgRuntimeRequest(pkgId));

        public Task Shutdown() => _impl.Ask(new ShutdownRuntimesCmd());

        internal void OnRuntimeStopped(string runtimeId) => _impl.Tell(new RuntimeStoppedMsg(runtimeId));

        internal Task<PkgRuntimeModel> ConnectRuntime(string runtimeId, RpcSession session) => _impl.Ask<PkgRuntimeModel>(new ConnectRuntimeCmd(runtimeId, session));


        private class InitMsg
        {
            public AlgoServer Server { get; }

            public InitMsg(AlgoServer server)
            {
                Server = server;
            }
        }

        private class PkgRuntimeRequest
        {
            public string PkgId { get; }

            public PkgRuntimeRequest(string pkgId)
            {
                PkgId = pkgId;
            }
        }

        private class ShutdownRuntimesCmd { }

        private class RuntimeStoppedMsg
        {
            public string Id { get; }

            public RuntimeStoppedMsg(string id)
            {
                Id = id;
            }
        }

        private class ConnectRuntimeCmd
        {
            public string Id { get; }

            public RpcSession Session { get; }

            public ConnectRuntimeCmd(string id, RpcSession session)
            {
                Id = id;
                Session = session;
            }
        }


        private class Impl : Actor
        {
            private readonly Dictionary<string, PkgRuntimeModel> _runtimeMap = new Dictionary<string, PkgRuntimeModel>();
            private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();

            private AlgoServer _server;


            public Impl()
            {
                Receive<PackageVersionUpdate>(OnPackageVersionUpdate);
                Receive<PkgRuntimeRequest, PkgRuntimeModel>(GetRuntime);
                Receive<ShutdownRuntimesCmd>(Shutdown);
                Receive<RuntimeStoppedMsg>(OnRuntimeStopped);
                Receive<ConnectRuntimeCmd>(ConnectRuntime);
            }


            protected override void ActorInit(object initMsg)
            {
                var msg = (InitMsg)initMsg;
                _server = msg.Server;
                _server.PkgStorage.PackageVersionUpdated.Subscribe(Self);
            }


            private void OnPackageVersionUpdate(PackageVersionUpdate update)
            {
                var pkgId = update.PackageId;
                var pkgRefId = update.LatestPkgRefId;

                if (_pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId))
                {
                    _runtimeMap[runtimeId].MarkForShutdown();
                }

                if (string.IsNullOrEmpty(pkgRefId))
                    return;

                runtimeId = pkgRefId.Replace('/', '-');

                _pkgRuntimeMap[pkgId] = runtimeId;
                _runtimeMap[runtimeId] = new PkgRuntimeModel(runtimeId, pkgId, pkgRefId, _server);
            }

            private PkgRuntimeModel GetRuntime(PkgRuntimeRequest request)
            {
                return _pkgRuntimeMap.TryGetValue(request.PkgId, out var runtimeId) ? _runtimeMap[runtimeId] : null;
            }

            private async Task Shutdown(ShutdownRuntimesCmd cmd)
            {
                _logger.Debug("Runtimes stopping...");

                await Task.WhenAll(_runtimeMap.Values.Select(r => 
                    r.Stop("Server shutdown")
                        .OnException(ex => _logger.Error(ex, $"Failed to stop runtime {r.Id}"))).ToArray());

                _logger.Debug("Runtimes stopped");
            }

            private void OnRuntimeStopped(RuntimeStoppedMsg msg)
            {
                _runtimeMap.Remove(msg.Id);
            }

            private async Task<PkgRuntimeModel> ConnectRuntime(ConnectRuntimeCmd cmd)
            {
                if (!_runtimeMap.TryGetValue(cmd.Id, out var runtime))
                    return null;

                return await runtime.OnConnect(cmd.Session) ? runtime : null;
            }
        }
    }
}

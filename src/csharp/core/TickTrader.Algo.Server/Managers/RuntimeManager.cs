using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class RuntimeManager : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly AlgoServer _server;
        private readonly Dictionary<string, PkgRuntimeModel> _runtimeMap = new Dictionary<string, PkgRuntimeModel>();
        private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();


        private RuntimeManager(AlgoServer server)
        {
            _server = server;

            Receive<PackageVersionUpdate>(OnPackageVersionUpdate);
            Receive<PkgRuntimeRequest, PkgRuntimeModel>(GetRuntime);
            Receive<ShutdownRuntimesCmd>(Shutdown);
            Receive<RuntimeStoppedMsg>(OnRuntimeStopped);
            Receive<ConnectRuntimeCmd>(ConnectRuntime);
        }


        public static IActorRef Create(AlgoServer server)
        {
            return ActorSystem.SpawnLocal(() => new RuntimeManager(server), nameof(RuntimeManager));
        }


        protected override void ActorInit(object initMsg)
        {
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
            _runtimeMap[runtimeId] = new PkgRuntimeModel(PkgRuntimeActor.Create(runtimeId, pkgId, pkgRefId, _server));
        }

        private PkgRuntimeModel GetRuntime(PkgRuntimeRequest request)
        {
            return _pkgRuntimeMap.TryGetValue(request.PkgId, out var runtimeId) ? _runtimeMap[runtimeId] : null;
        }

        private async Task Shutdown(ShutdownRuntimesCmd cmd)
        {
            _logger.Debug("Runtimes stopping...");

            await Task.WhenAll(_runtimeMap.Select(r =>
                r.Value.Stop("Server shutdown")
                    .OnException(ex => _logger.Error(ex, $"Failed to stop runtime {r.Key}"))).ToArray());

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


        internal class PkgRuntimeRequest
        {
            public string PkgId { get; }

            public PkgRuntimeRequest(string pkgId)
            {
                PkgId = pkgId;
            }
        }

        internal class ShutdownRuntimesCmd { }

        internal class RuntimeStoppedMsg
        {
            public string Id { get; }

            public RuntimeStoppedMsg(string id)
            {
                Id = id;
            }
        }

        internal class ConnectRuntimeCmd
        {
            public string Id { get; }

            public RpcSession Session { get; }

            public ConnectRuntimeCmd(string id, RpcSession session)
            {
                Id = id;
                Session = session;
            }
        }
    }
}

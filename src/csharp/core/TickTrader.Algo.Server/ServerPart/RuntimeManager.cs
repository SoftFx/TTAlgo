using Google.Protobuf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class RuntimeManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, IActorRef> _runtimeMap = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();


        public RuntimeManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Runtimes stopping...");

            await Task.WhenAll(_runtimeMap.Select(r =>
                r.Value.Ask(new PkgRuntimeActor.StopRuntimeCmd("Server shutdown"))
                    .OnException(ex => _logger.Error(ex, $"Failed to stop runtime {r.Key}"))).ToArray());

            _logger.Debug("Runtimes stopped");
        }

        public void OnRuntimeStopped(string id)
        {
            if (!_runtimeMap.TryGetValue(id, out var runtime))
                return;

            _runtimeMap.Remove(id);
            ActorSystem.StopActor(runtime)
                .OnException(ex => _logger.Error(ex, $"Failed to stop actor {runtime.Name}"));
        }

        public async Task<PkgRuntimeModel> ConnectRuntime(string id, RpcSession session)
        {
            if (!_runtimeMap.TryGetValue(id, out var runtime))
                return null;

            var connected = await runtime.Ask<bool>(new PkgRuntimeActor.ConnectSessionCmd(session));
            return  connected ? new PkgRuntimeModel(runtime) : null;
        }

        public PkgRuntimeModel GetPkgRuntime(string pkgId)
        {
            return _pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId) ? new PkgRuntimeModel(_runtimeMap[runtimeId]) : null;
        }

        public void CreateRuntime(string id, PackageRef pkgRef)
        {
            var pkgId = pkgRef.PkgId;
            var pkgBytes = pkgRef.PkgBytes;
            var pkgBin = pkgBytes == null ? ByteString.Empty : ByteString.CopyFrom(pkgBytes);
            var config = new RuntimeConfig { Id = id, PackageId = pkgId, PackageBinary = pkgBin, PackageIdentity = pkgRef.PkgInfo.Identity };

            _pkgRuntimeMap[pkgId] = id;
            _runtimeMap[id] = PkgRuntimeActor.Create(_server, config);
        }


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

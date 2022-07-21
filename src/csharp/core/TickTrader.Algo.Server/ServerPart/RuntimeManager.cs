using Google.Protobuf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.PkgStorage;

namespace TickTrader.Algo.Server
{
    internal class RuntimeManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, IActorRef> _runtimeMap = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _runtumeVersions = new Dictionary<string, int>();


        public RuntimeManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Runtimes stopping...");

            await Task.WhenAll(_runtimeMap.Select(r =>
                RuntimeControlModel.Shutdown(r.Value)
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

        public IActorRef GetRuntime(string id)
        {
            _runtimeMap.TryGetValue(id, out var runtime);
            return runtime;
        }

        public IActorRef GetPkgRuntime(string pkgId)
        {
            return _pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId) ? _runtimeMap[runtimeId] : null;
        }

        public string GetPkgRuntimeId(string pkgId)
        {
            _pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId);
            return runtimeId;
        }

        public string CreatePkgRuntime(PackageRef pkgRef)
        {
            var pkgId = pkgRef.PkgId;
            var id = GenerateRuntimeId(pkgId);
            var pkgBytes = pkgRef.PkgBytes;
            var pkgBin = pkgBytes == null ? ByteString.Empty : ByteString.CopyFrom(pkgBytes);
            var config = new RuntimeConfig { Id = id, PackageId = pkgId, PackageBinary = pkgBin };

            _pkgRuntimeMap[pkgId] = id;
            _runtimeMap[id] = RuntimeControlActor.Create(_server, config, pkgRef.PkgInfo);

            return id;
        }

        public void MarkPkgRuntimeObsolete(string pkgId)
        {
            if (_pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId))
            {
                _pkgRuntimeMap.Remove(pkgId);
                RuntimeControlModel.MarkObsolete(_runtimeMap[runtimeId]);
            }
        }


        private string GenerateRuntimeId(string pkgId)
        {
            if (!_runtumeVersions.TryGetValue(pkgId, out var currentVersion))
                currentVersion = -1;

            currentVersion++;
            _runtumeVersions[pkgId] = currentVersion;
            var runtimeId = $"{pkgId.Replace('/', '-')}-{currentVersion}";
            return runtimeId;
        }


        internal class RuntimeStoppedMsg
        {
            public string Id { get; }

            public RuntimeStoppedMsg(string id)
            {
                Id = id;
            }
        }
    }
}

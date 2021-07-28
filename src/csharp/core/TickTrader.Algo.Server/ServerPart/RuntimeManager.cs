using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class RuntimeManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly AlgoServerPrivate _server;
        private readonly Dictionary<string, PkgRuntimeModel> _runtimeMap = new Dictionary<string, PkgRuntimeModel>();
        private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();


        public RuntimeManager(AlgoServerPrivate server)
        {
            _server = server;
        }


        public async Task Shutdown()
        {
            _logger.Debug("Runtimes stopping...");

            await Task.WhenAll(_runtimeMap.Select(r =>
                r.Value.Stop("Server shutdown")
                    .OnException(ex => _logger.Error(ex, $"Failed to stop runtime {r.Key}"))).ToArray());

            _logger.Debug("Runtimes stopped");
        }

        public void OnRuntimeStopped(string id)
        {
            _runtimeMap.Remove(id);
        }

        public async Task<PkgRuntimeModel> ConnectRuntime(string id, RpcSession session)
        {
            if (!_runtimeMap.TryGetValue(id, out var runtime))
                return null;

            return await runtime.OnConnect(session) ? runtime : null;
        }

        public PkgRuntimeModel GetPkgRuntime(string pkgId)
        {
            return _pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId) ? _runtimeMap[runtimeId] : null;
        }

        public void CreateRuntime(RuntimeConfig config)
        {
            var id = config.Id;
            var pkgId = config.PackageId;

            _pkgRuntimeMap[pkgId] = id;
            _runtimeMap[id] = new PkgRuntimeModel(PkgRuntimeActor.Create(config, _server));
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

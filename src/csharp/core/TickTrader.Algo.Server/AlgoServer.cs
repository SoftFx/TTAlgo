using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Server
{
    public class AlgoServer : IRpcHost
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServer>();

        private readonly PackageStorage _pkgStorage = new PackageStorage();
        private readonly Dictionary<string, RuntimeModel> _runtimeMap = new Dictionary<string, RuntimeModel>();
        private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();
        private readonly Dictionary<string, ExecutorModel> _executorsMap = new Dictionary<string, ExecutorModel>();
        private readonly Dictionary<string, IAccountProxy> _accountsMap = new Dictionary<string, IAccountProxy>();
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "127.0.0.1";

        public int BoundPort => _rpcServer.BoundPort;

        public PackageStorage PackageStorage => _pkgStorage;


        public AlgoServer()
        {
            _rpcServer = new RpcServer(new TcpFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);
        }

        public async Task Stop()
        {
            _logger.Debug("Stopping...");

            await _pkgStorage.Stop();

            Task[] stopRuntimeTasks;
            lock (_runtimeMap)
            {
                stopRuntimeTasks = _runtimeMap.Values.Select(r => StopRuntime(r)).ToArray();
            }
            _logger.Debug("Runtimes stopping...");
            await Task.WhenAll(stopRuntimeTasks);
            _logger.Debug("Runtimes stopped");

            await _rpcServer.Stop();

            _logger.Debug("Stopped");
        }

        public async Task<ExecutorModel> CreateExecutor(PluginConfig config, string accountId)
        {
            if (_executorsMap.ContainsKey(config.InstanceId))
                throw new ArgumentException("Duplicate instance id");

            var packageId = config.Key.PackageId;

            var runtime = await GetPackageRuntime(packageId);
            var executor = runtime.CreateExecutor(config, accountId);
            _executorsMap.Add(executor.Id, executor);
            return executor;
        }

        public bool RegisterAccountProxy(IAccountProxy account)
        {
            if (_accountsMap.ContainsKey(account.Id))
                return false;

            _accountsMap.Add(account.Id, account);
            return true;
        }


        internal bool TryGetRuntime(string runtimeId, out RuntimeModel runtime)
        {
            return _runtimeMap.TryGetValue(runtimeId, out runtime);
        }

        internal bool TryGetExecutor(string executorId, out ExecutorModel executor)
        {
            return _executorsMap.TryGetValue(executorId, out executor);
        }

        internal bool TryGetAccount(string accountId, out IAccountProxy account)
        {
            return _accountsMap.TryGetValue(accountId, out account);
        }

        internal void OnExecutorStopped(string executorId)
        {
            _executorsMap.Remove(executorId);
        }

        internal void OnRuntimeStopped(string runtimeId)
        {
            lock (_runtimeMap)
            {
                _runtimeMap.Remove(runtimeId);
            }
        }


        private async Task<RuntimeModel> GetPackageRuntime(string pkgId)
        {
            if (_pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId))
            {
                var rt = _runtimeMap[runtimeId];
                await rt.WaitForLaunch();
                return rt;
            }

            var pkgRef = await _pkgStorage.GetPackageRef(pkgId);
            if (pkgRef == null)
                throw new ArgumentException("Package not found", nameof(pkgId));

            runtimeId = pkgRef.Id;
            var runtime = await StartRuntime(runtimeId, pkgRef);
            _pkgRuntimeMap[pkgId] = runtimeId;
            return runtime;
        }

        private async Task<RuntimeModel> GetInstanceRuntime(string instanceId, string pkgId)
        {
            var runtimeId = $"inst/{instanceId}";
            if (_runtimeMap.ContainsKey(runtimeId))
                throw new ArgumentException("Instance runtime already exists", nameof(instanceId));

            var pkgRef = await _pkgStorage.GetPackageRef(pkgId);
            if (pkgRef == null)
                throw new ArgumentException("Package not found", nameof(pkgId));

            return await StartRuntime(runtimeId, pkgRef);
        }

        private async Task<RuntimeModel> StartRuntime(string runtimeId, AlgoPackageRef pkgRef)
        {
            RuntimeModel runtime = null;
            try
            {
                runtime = new RuntimeModel(this, runtimeId, pkgRef);
                _runtimeMap[runtimeId] = runtime;
                await runtime.Start(Address, BoundPort);
                await runtime.WaitForLaunch();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start runtime {runtimeId} for {pkgRef.Id}");
                pkgRef.DecrementRef();
            }
            return runtime;
        }

        private async Task StopRuntime(RuntimeModel runtime)
        {
            try
            {
                await runtime.Stop("Server shutdown");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to stop runtime {runtime.Id}");
            }
        }


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            switch (protocol.Url)
            {
                case KnownProtocolUrls.RuntimeV1:
                    return new ServerRuntimeV1Handler(this);
            }
            return null;
        }

        #endregion IRpcHost implementation
    }
}

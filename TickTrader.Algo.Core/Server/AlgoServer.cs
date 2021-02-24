using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Core
{
    public class AlgoServer : IRpcHost
    {
        private static readonly IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger<AlgoServer>();

        private readonly Dictionary<string, string> _runtimeIdMap;
        private readonly Dictionary<string, RuntimeModel> _runtimesMap;
        private readonly Dictionary<string, ExecutorModel> _executorsMap;
        private readonly Dictionary<string, IAccountProxy> _accountsMap;
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "127.0.0.1";

        public int BoundPort => _rpcServer.BoundPort;


        public AlgoServer()
        {
            _runtimeIdMap = new Dictionary<string, string>();
            _runtimesMap = new Dictionary<string, RuntimeModel>();
            _executorsMap = new Dictionary<string, ExecutorModel>();
            _accountsMap = new Dictionary<string, IAccountProxy>();

            _rpcServer = new RpcServer(new TcpFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);
        }

        public async Task Stop()
        {
            await Task.WhenAll(_runtimesMap.Values.Select(r => StopRuntime(r)));
            await _rpcServer.Stop();
        }

        public RuntimeModel CreateRuntime(AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            var id = Guid.NewGuid().ToString("N");
            var runtime = new RuntimeModel(this, id, pluginRef);
            _runtimesMap.Add(id, runtime);
            return runtime;
        }

        public async Task<ExecutorModel> CreateExecutor(AlgoPluginRef pluginRef, PluginConfig config, string accountId)
        {
            if (_executorsMap.ContainsKey(config.InstanceId))
                throw new ArgumentException("Duplicate instance id");

            var runtime = await GetOrCreateRuntime(pluginRef);
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
            return _runtimesMap.TryGetValue(runtimeId, out runtime);
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


        private string GenerateRuntimeId(string packagePath)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(packagePath);
            int cnt = 0;
            var id = $"{fileName} - {cnt++}";
            while (_runtimesMap.ContainsKey(id)) id = $"{fileName} - {cnt++}";
            return id;
        }

        private async Task<RuntimeModel> GetOrCreateRuntime(AlgoPluginRef pluginRef)
        {
            var path = pluginRef.PackagePath;
            if (_runtimeIdMap.TryGetValue(pluginRef.PackagePath, out var runtimeId))
            {
                var startedRuntime = _runtimesMap[runtimeId];
                await startedRuntime.WaitForLaunch();
                return startedRuntime;
            }
            runtimeId = GenerateRuntimeId(pluginRef.PackagePath);
            _runtimeIdMap[path] = runtimeId;
            var runtime = new RuntimeModel(this, runtimeId, pluginRef);
            _runtimesMap[runtimeId] = runtime;
            await runtime.Start(Address, BoundPort);

            return runtime;
        }

        private async Task StopRuntime(RuntimeModel runtime)
        {
            try
            {
                await runtime.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to stop runtime {runtime.Id}", ex);
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

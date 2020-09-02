using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Core
{
    public class AlgoServer : IRpcHost
    {
        private readonly Dictionary<string, PluginExecutor> _executorsMap;
        private readonly Dictionary<string, RuntimeModel> _runtimesMap;
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "127.0.0.1";

        public int BoundPort => _rpcServer.BoundPort;


        public AlgoServer()
        {
            _executorsMap = new Dictionary<string, PluginExecutor>();
            _runtimesMap = new Dictionary<string, RuntimeModel>();
            _rpcServer = new RpcServer(new TcpFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);
        }

        public async Task Stop()
        {
            await _rpcServer.Stop();
        }

        public PluginExecutor CreateExecutor(AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            var id = Guid.NewGuid().ToString("N");
            var executor = new PluginExecutor(id, pluginRef, updatesSync);
            _executorsMap.Add(id, executor);
            return executor;
        }

        public RuntimeModel CreateRuntime(AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            var id = Guid.NewGuid().ToString("N");
            var runtime = new RuntimeModel(id, pluginRef, updatesSync);
            _runtimesMap.Add(id, runtime);
            return runtime;
        }


        internal bool TryGetExecutor(string executorId, out PluginExecutor executor)
        {
            return _executorsMap.TryGetValue(executorId, out executor);
        }

        internal bool TryGetRuntime(string executorId, out RuntimeModel runtime)
        {
            return _runtimesMap.TryGetValue(executorId, out runtime);
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

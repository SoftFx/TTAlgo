using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverGrpc;

namespace TickTrader.Algo.Core
{
    public class AlgoServer : IRpcHost
    {
        private readonly Dictionary<string, PluginExecutor> _executorsMap;
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "localhost";

        public int BoundPort => _rpcServer.BoundPort;


        public AlgoServer()
        {
            _executorsMap = new Dictionary<string, PluginExecutor>();
            _rpcServer = new RpcServer(new GrpcFactory(), this);
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


        internal bool TryGetExecutor(string executorId, out PluginExecutor executor)
        {
            return _executorsMap.TryGetValue(executorId, out executor);
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

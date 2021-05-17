using System;
using System.Threading;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Core
{
    public class PluginLauncher : CrossDomainObject, IRpcHost
    {
        private readonly RpcClient _client;


        public PluginExecutorCore Core { get; private set; }

        internal PluginRuntimeV1Handler Handler { get; private set; }


        public PluginLauncher()
        {
            _client = new RpcClient(new TcpFactory(), this, new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 });
        }


        public void Launch(string address, int port, string executorId)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    await _client.Connect(address, port);
                    await Handler.AttachRuntime(executorId);
                }
                catch (Exception) { }
            });
        }

        public PluginExecutorCore CreateExecutor(string pluginId)
        {
            Core = new PluginExecutorCore(pluginId);
            //Handler = new PluginRuntimeV1Handler(Core);
            return Core;
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
                    return Handler;
            }
            return null;
        }

        #endregion IRpcHost implementation
    }
}

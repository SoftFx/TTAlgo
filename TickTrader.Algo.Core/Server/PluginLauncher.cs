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

        internal UnitRuntimeV1Handler Handler { get; private set; }


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
            //Handler = new UnitRuntimeV1Handler(Core);
            return Core;
        }

        public void ConfigureRuntime()
        {
            //var provider = new RuntimeInfoProvider(Handler);
            //Core.Metadata = provider;
            //Core.AccInfoProvider = provider;
            //Core.TradeExecutor = provider;
            //Core.TradeHistoryProvider = provider;
            //Core.Feed = provider;
            //Core.FeedHistory = provider;
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

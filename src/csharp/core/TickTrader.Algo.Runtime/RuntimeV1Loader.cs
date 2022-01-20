using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Runtime
{
    public class RuntimeV1Loader : IRpcHost, IRuntimeProxy
    {
        public const int AbortTimeout = 10000;

        private static readonly ProtocolSpec ExpectedProtocol = new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 };

        private readonly RpcClient _client;
        private readonly PluginRuntimeV1Handler _handler;

        private IAlgoLogger _logger;
        private string _id;
        private IActorRef _runtime;


        public RuntimeV1Loader()
        {
            _client = new RpcClient(new TcpFactory(), this, ExpectedProtocol);
            _handler = new PluginRuntimeV1Handler(this);
        }


        public async Task Init(string address, int port, string proxyId)
        {
            _logger = AlgoLoggerFactory.GetLogger<RuntimeV1Loader>();
            _id = proxyId;

            await _client.Connect(address, port).ConfigureAwait(false);
            var attached = await _handler.AttachRuntime(proxyId).ConfigureAwait(false);
            if (!attached)
            {
                _logger.Error("Runtime was not attached");
                await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
            }
        }

        public async Task Deinit()
        {
            await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
        }

        public Task WhenFinished()
        {
            return _handler.WhenDisconnected();
        }

        public async Task Start(StartRuntimeRequest request)
        {
            _runtime = RuntimeV1Actor.Create(_id, _handler);

            await _runtime.Ask(request);
        }

        public Task Stop(StopRuntimeRequest request)
        {
            return _runtime.Ask(request);
        }

        public Task StartExecutor(StartExecutorRequest request)
        {
            return _runtime.Ask(request);
        }

        public Task StopExecutor(StopExecutorRequest request)
        {
            return _runtime.Ask(request);
        }


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            return ExpectedProtocol.Url == protocol.Url ? _handler : null;
        }

        #endregion IRpcHost implementation
    }
}

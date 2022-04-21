using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.BacktesterV1Host
{
    public class BacktesterV1Loader : IRpcHost
    {
        public const int AbortTimeout = 10000;

        private static readonly ProtocolSpec ExpectedProtocol = new ProtocolSpec { Url = KnownProtocolUrls.BacktesterV1, MajorVerion = 1, MinorVerion = 0 };

        private readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<BacktesterV1Loader>();
        private readonly BacktesterV1HostHandler _handler;
        private readonly RpcClient _client;


        public BacktesterV1Loader(BacktesterV1HostHandler handler)
        {
            _handler = handler;
            _client = new RpcClient(new TcpFactory(), this, ExpectedProtocol);
        }


        public async Task Init(string address, int port)
        {
            await _client.Connect(address, port).ConfigureAwait(false);
            var attached = await _handler.AttachBacktester().ConfigureAwait(false);
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

        public Task WhenFinished() => _handler.WhenDisconnected();


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

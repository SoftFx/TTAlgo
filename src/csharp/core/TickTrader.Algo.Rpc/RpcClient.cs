using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public class RpcClient
    {
        private readonly ITransportFactory _transportFactory;
        private readonly IRpcHost _rpcHost;
        private readonly ProtocolSpec _protocolSpec;

        private ITransportClient _transportClient;


        public RpcSession Session { get; private set; }


        public RpcClient(ITransportFactory transportFactory, IRpcHost rpcHost, ProtocolSpec protocolSpec)
        {
            _transportFactory = transportFactory;
            _rpcHost = rpcHost;
            _protocolSpec = protocolSpec;
        }


        public async Task Connect(string address, int port)
        {
            _transportClient = _transportFactory.CreateClient();
            await _transportClient.Start(address, port);
            var transport = _transportClient.Transport;
            var session = new RpcSession(transport, _rpcHost);
            await session.Connect(_protocolSpec);

            Session = session;
        }

        public async Task Disconnect(string reason)
        {
            await Session.Disconnect(reason);
            await _transportClient.Stop();
            _transportClient = null;
        }
    }
}

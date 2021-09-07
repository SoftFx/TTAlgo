using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async;

namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcServer : OverGrpc.OverGrpcBase, ITransportServer
    {
        private readonly ChannelEventSource<ITransportProxy> _newConnectionSink = new ChannelEventSource<ITransportProxy>();
        private Server _server;
        private bool _acceptNewRequests;


        public IEventSource<ITransportProxy> NewConnectionAdded => _newConnectionSink;

        public int BoundPort { get; private set; }


        public GrpcServer()
        {
            BoundPort = -1;
        }


        public Task Start(string address, int port)
        {
            _server = new Server()
            {
                Services = { OverGrpc.BindService(this) },
                Ports = { new ServerPort(address, port, ServerCredentials.Insecure) },
            };
            _server.Start();
            BoundPort = _server.Ports.First().BoundPort;
            _acceptNewRequests = true;
            return Task.FromResult(this);
        }

        public Task StopNewConnections()
        {
            _acceptNewRequests = false;
            BoundPort = -1;
            _newConnectionSink.Dispose();

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return _server.ShutdownAsync();
        }

        public override Task OpenDuplexChannel(IAsyncStreamReader<MessagePage> requestStream, IServerStreamWriter<MessagePage> responseStream, ServerCallContext context)
        {
            if (!_acceptNewRequests)
                return Task.CompletedTask;

            var session = new GrpcSession(requestStream, responseStream);
            _newConnectionSink.Send(session);
            return session.Completion;
        }
    }
}

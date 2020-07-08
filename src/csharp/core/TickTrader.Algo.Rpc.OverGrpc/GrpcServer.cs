using Grpc.Core;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcServer : OverGrpc.OverGrpcBase, ITransportServer
    {
        private readonly Subject<ITransportProxy> _newConnectionSubject;
        private Server _server;


        public IObservable<ITransportProxy> ObserveNewConnentions => _newConnectionSubject;

        public int BoundPort { get; private set; }


        public GrpcServer()
        {
            _newConnectionSubject = new Subject<ITransportProxy>();
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
            return Task.FromResult(this);
        }

        public async Task Stop()
        {
            await _server.ShutdownAsync();
            BoundPort = -1;
            _newConnectionSubject.OnCompleted();
            _newConnectionSubject.Dispose();
        }

        public override Task OpenDuplexChannel(IAsyncStreamReader<MessagePage> requestStream, IServerStreamWriter<MessagePage> responseStream, ServerCallContext context)
        {
            var session = new GrpcSession(requestStream, responseStream);
            _newConnectionSubject.OnNext(session);
            return session.Completion;
        }
    }
}

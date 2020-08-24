using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpServer : ITransportServer
    {
        private readonly Subject<ITransportProxy> _newConnectionSubject;

        private Socket _listenSocket;
        private CancellationTokenSource _cancelTokenSrc;
        private Task _acceptTask;


        public IObservable<ITransportProxy> ObserveNewConnentions => _newConnectionSubject;

        public int BoundPort { get; private set; }


        public TcpServer()
        {
            _newConnectionSubject = new Subject<ITransportProxy>();
            BoundPort = -1;
        }


        public Task Start(string address, int port)
        {
            _cancelTokenSrc = new CancellationTokenSource();

            var ip = IPAddress.Parse(address);
            var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ip, port));
            BoundPort = ((IPEndPoint)socket.LocalEndPoint).Port;
            _listenSocket = socket;

            _acceptTask = AcceptLoop(socket, _cancelTokenSrc.Token);

            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            _cancelTokenSrc.Cancel();
            await _acceptTask;

            BoundPort = -1;
            _newConnectionSubject.OnCompleted();
            _newConnectionSubject.Dispose();

            _listenSocket.Close();
        }


        private async Task AcceptLoop(Socket listenSocket, CancellationToken cancelToken)
        {
            listenSocket.Listen(1000);

            while (!cancelToken.IsCancellationRequested)
            {
                var clientSocket = await listenSocket.AcceptAsync().ConfigureAwait(false);

                var session = new TcpSession(clientSocket);
                await session.Start();
                _newConnectionSubject.OnNext(session);
            }
        }
    }
}

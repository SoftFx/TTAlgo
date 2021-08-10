using ActorSharp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpContext : Actor { }


    public class TcpServer : ITransportServer
    {
        private readonly ChannelEventSource<ITransportProxy> _newConnectionSink = new ChannelEventSource<ITransportProxy>();
        private readonly Ref<TcpContext> _context;

        private Socket _listenSocket;
        private CancellationTokenSource _cancelTokenSrc;
        private Task _acceptTask;


        public IEventSource<ITransportProxy> NewConnectionAdded => _newConnectionSink;

        public int BoundPort { get; private set; }


        public TcpServer()
        {
            BoundPort = -1;
            _context = Actor.SpawnLocal<TcpContext>(null, $"TcpServer {Guid.NewGuid().ToString("N")}");
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

        public async Task StopNewConnections()
        {
            _cancelTokenSrc.Cancel();
            _listenSocket.Close();
            try
            {
                await _acceptTask;
            }
            catch (SocketException) { }

            BoundPort = -1;
            _newConnectionSink.Dispose();
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }


        private async Task AcceptLoop(Socket listenSocket, CancellationToken cancelToken)
        {
            listenSocket.Listen(1000);

            while (!cancelToken.IsCancellationRequested)
            {
                var clientSocket = await listenSocket.AcceptAsync().ConfigureAwait(false);

                var session = new TcpSession(clientSocket, _context);
                await session.Start();
                _newConnectionSink.Send(session);
            }
        }
    }
}

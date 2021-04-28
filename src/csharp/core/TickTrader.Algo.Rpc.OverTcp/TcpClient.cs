using ActorSharp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpClient : ITransportClient
    {
        private TcpSession _session;
        private readonly Ref<TcpContext> _context;


        public ITransportProxy Transport => _session;


        public TcpClient()
        {
            _context = Actor.SpawnLocal<TcpContext>(null, $"TcpClient {Guid.NewGuid().ToString("N")}");
        }


        public async Task Start(string address, int port)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var ip = IPAddress.Parse(address);
            var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await socket.ConnectAsync(ip, port).ConfigureAwait(false);

            _session = new TcpSession(socket, _context);
            await _session.Start();
        }

        public Task Stop()
        {
            return _session.Stop();
        }
    }
}

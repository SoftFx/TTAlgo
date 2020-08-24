using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpClient : ITransportClient
    {
        private TcpSession _session;


        public ITransportProxy Transport => _session;


        public TcpClient()
        {
        }


        public async Task Start(string address, int port)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var ip = IPAddress.Parse(address);
            var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await socket.ConnectAsync(ip, port).ConfigureAwait(false);

            _session = new TcpSession(socket);
            await _session.Start();
        }

        public Task Stop()
        {
            return _session.Stop();
        }
    }
}

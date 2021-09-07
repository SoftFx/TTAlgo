using Grpc.Core;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcClient : ITransportClient
    {
        private Channel _channel;
        private OverGrpc.OverGrpcClient _client;
        private AsyncDuplexStreamingCall<MessagePage, MessagePage> _duplexCall;
        private GrpcSession _session;


        public ITransportProxy Transport { get; private set; }


        public async Task Start(string address, int port)
        {
            _channel = new Channel(address, port, ChannelCredentials.Insecure);
            await _channel.ConnectAsync();
            _client = new OverGrpc.OverGrpcClient(_channel);

            _duplexCall = _client.OpenDuplexChannel();
            _session = new GrpcSession(_duplexCall.ResponseStream, _duplexCall.RequestStream);
            Transport = _session;
        }

        public async Task Stop()
        {
            Transport = null;
            await _duplexCall.RequestStream.CompleteAsync();
            await _session.Close();
            await _channel.ShutdownAsync();
        }
    }
}

using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;

namespace TickTrader.Algo.Rpc
{
    public interface ITransportProxy
    {
        ChannelReader<RpcMessage> ReadChannel { get; }

        ChannelWriter<RpcMessage> WriteChannel { get; }

        Action<Exception, string> DebugErrorCallback { set; }

        bool EnableTraceLog { get; set; }


        Task Close();
    }

    public interface ITransportServer
    {
        IEventSource<ITransportProxy> NewConnectionAdded { get; }

        int BoundPort { get; }


        Task Start(string address, int port);

        Task StopNewConnections();

        Task Stop();
    }

    public interface ITransportClient
    {
        ITransportProxy Transport { get; }

        Task Start(string address, int port);

        Task Stop();
    }

    public interface ITransportFactory
    {
        ITransportServer CreateServer();

        ITransportClient CreateClient();
    }
}

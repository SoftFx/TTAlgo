using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public interface ITransportProxy
    {
        void AttachListener(IObserver<RpcMessage> msgListener);

        void SendMessage(RpcMessage payload);

        Task Close();
    }

    public interface ITransportServer
    {
        IObservable<ITransportProxy> ObserveNewConnentions { get; }

        int BoundPort { get; }


        Task Start(string address, int port);

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

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public interface IContextFactory
    {
        SynchronizationContext CreateContext();
    }

    public interface IMsgDispatcher
    {
        void PostMessage(object msg);

        void Start(Action<object> msgHandler);

        Task Stop();
    }

    public interface IMsgDispatcherFactory
    {
        IMsgDispatcher CreateDispatcher(string actorName, int maxBatch);
    }


    internal interface IAskMsg
    {
        object Request { get; }

        void SetResponse(object result);
    }

    internal interface IMsgHandler
    {
        string Type { get; }

        object Run(object msg);
    }
}

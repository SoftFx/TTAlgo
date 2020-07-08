using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public interface IRpcResponseContext
    {
        bool OnNext(Any payload);
    }


    public class RpcResponseContext : IRpcResponseContext
    {
        public object State { get; }

        Func<object, Any, bool> ResponseHandler { get; }


        public RpcResponseContext(object state, Func<object, Any, bool> responseHandler)
        {
            State = state;
            ResponseHandler = responseHandler;
        }


        public bool OnNext(Any payload)
        {
            return ResponseHandler(State, payload);
        }
    }


    public class RpcResponseTaskContext<T> : IRpcResponseContext
    {
        public TaskCompletionSource<T> TaskSrc { get; }

        Func<TaskCompletionSource<T>, Any, bool> ResponseHandler { get; }


        public RpcResponseTaskContext(Func<TaskCompletionSource<T>, Any, bool> responseHandler)
        {
            TaskSrc = new TaskCompletionSource<T>();
            ResponseHandler = responseHandler;
        }

        public bool OnNext(Any payload)
        {
            return ResponseHandler(TaskSrc, payload);
        }
    }
}

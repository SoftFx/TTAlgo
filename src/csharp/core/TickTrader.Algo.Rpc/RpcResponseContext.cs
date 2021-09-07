using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
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

    public class RpcListResponseTaskContext<T> : IRpcResponseContext, IObserver<RepeatedField<T>>
    {
        private List<T> _list;

        public TaskCompletionSource<List<T>> TaskSrc { get; }

        Func<IObserver<RepeatedField<T>>, Any, bool> ResponseHandler { get; }

        public RpcListResponseTaskContext(Func<IObserver<RepeatedField<T>>, Any, bool> responseHandler)
        {
            TaskSrc = new TaskCompletionSource<List<T>>();
            ResponseHandler = responseHandler;

            _list = new List<T>();
        }

        public bool OnNext(Any payload)
        {
            return ResponseHandler(this, payload);
        }

        public void OnCompleted()
        {
            TaskSrc.TrySetResult(_list);
        }

        public void OnError(Exception error)
        {
            TaskSrc.TrySetException(error);
        }

        public void OnNext(RepeatedField<T> items)
        {
            _list.AddRange(items);
        }
    }
}

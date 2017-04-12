using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Machinarium.ActorModel
{
    public interface Channel
    {
        bool IsClosed { get; }
        event Action<Channel> Closed;
    }

    public interface TxChannel<T> : Channel
    {
        Task Write(T item);
    }

    public interface RxChannel<T> : Channel
    {
        Task<T> Read();
    }

    //public interface DuplexChannel<T> : TxChannel<T>, RxChannel<T>
    //{
    //}

    //internal class DataflowChannel<T> : TxChannel<T>, RxChannel<T>
    //{
    //    private TransformBlock<T, object> block;
    //    private TaskCompletionSource<T> rxTask;

    //    public DataflowChannel(DataflowScope scope)
    //    {
    //        block = new TransformBlock<T, object>(a => { });
    //        block.LinkTo(scope.QueueBlock);
    //    }

    //    public bool IsClosed
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public event Action<Channel> Closed;

    //    public Task<T> Read()
    //    {
            
    //        if (block.TryReceive(out result))
    //            return result;
    //    }

    //    public Task Write(T item)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    internal class OutChannel<T>
    {
    }

    //internal class Multicaster<T>
    //{
    //}
}

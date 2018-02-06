using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp
{
    public abstract class ActorRef
    {
        public abstract void PostMessage(object message);
    }

    public abstract class Ref<TActor> : ActorRef
    {
        //public abstract Link<TActor> OpenLink();

        public abstract void Send(Action<TActor> method);
        public abstract Task Call(Action<TActor> method);
        public abstract Task Call(Func<TActor, Task> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, TResult> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method);
        //public abstract Task OpenChannel<T>(Action<TActor, Channel<T>> method, Channel<T> channel);
        //public abstract Task<TResult> OpenChannel<T, TResult>(Func<TActor, Channel<T>, TResult> method, Channel<T> channel);
        public abstract Task OpenChannel<T>(Channel<T> channel, Action<TActor, Channel<T>> actorMethod);
        public abstract Task<TResult> OpenChannel<T, TResult>(Channel<T> channel, Func<TActor, Channel<T>, TResult> actorMethod);

        //internal abstract Task<IChannelWriter<T>> OpenTxChannel<T>(Action<TActor, IChannelReader<T>> method, int pageSize);
        //internal abstract Task<TResult> OpenRxChannel<T, TResult>(IChannelReader<T> channel, Func<TActor, IChannelWriter<T>, TResult> method, int pageSize);
        //internal abstract Task<IChannelReader<T>> OpenRxChannel<T>(Action<TActor, IChannelWriter<T>> method, int pageSize);
        //internal abstract Task<IChannelWriter<T>> OpenTxChannel<T>(Action<TActor, IChannelReader<T>> method, int pageSize);
        //internal abstract Task<TResult> OpenRxChannel<T, TResult>(IChannelReader<T> channel, Func<TActor, IChannelWriter<T>, TResult> method, int pageSize);
        //internal abstract Task<TResult> OpenTxChannel<T, TResult>(IChannelWriter<T> channel, Func<TActor, IChannelReader<T>, TResult> method, int pageSize);

        //internal abstract IChannelWriter<T> NewTxChannel<T>();
        //internal abstract IChannelReader<T> NewRxChannel<T>();

        //internal abstract IBlockingRef<TActor> Blocking { get; }

        //public abstract IRxChannel<T> Marshal<T>(ITxChannel<T> channel, int pageSize = 10);
        //public abstract ITxChannel<T> Marshal<T>(IRxChannel<T> channel, int pageSize = 10);
    }

    public abstract class Link<TActor>
    {
        public abstract IAwaitable SendSlim(Action<TActor> method);
        public abstract Task Send(Action<TActor> method);
        public abstract Task Call(Action<TActor> method);
        public abstract Task Call(Func<TActor, Task> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, TResult> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method);
    }

    public abstract class BlockingLink<TActor>
    {
        public abstract void Send(Action<TActor> method);
        public abstract void Call(Action<TActor> method);
        public abstract void Call(Func<TActor, Task> method);
        public abstract TResult Call<TResult>(Func<TActor, TResult> method);
        public abstract TResult Call<TResult>(Func<TActor, Task<TResult>> method);
    }

    //public struct OpenRxChannelResult<TChannel, TResult>
    //{
    //    public IRxChannel<TChannel> Channel { get; }
    //    public TResult Responce { get; }
    //}

    //public struct OpenTxChannelResult<TChannel, TResult>
    //{
    //    public ITxChannel<TChannel> Channel { get; }
    //    public TResult Responce { get; }
    //}

    //public class Ref<THandler>
    //    where THandler : Handler, new()
    //{
    //    private IActorRef _actorRef;

    //    internal Ref(IActorRef actorRef)
    //    {
    //        _actorRef = actorRef ?? throw new ArgumentNullException("actorRef");
    //    }

    //    public THandler CreateHandler()
    //    {
    //        var h = new THandler();
    //        h.Init(_actorRef);
    //        return h;
    //    }
    //}
}

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
        public abstract void Send(Action<TActor> method);
        public abstract Task Call(Action<TActor> method);
        public abstract Task Call(Func<TActor, Task> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, TResult> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method);
        public abstract Task OpenChannel<T>(Channel<T> channel, Action<TActor, Channel<T>> actorMethod);
        public abstract Task<TResult> OpenChannel<T, TResult>(Channel<T> channel, Func<TActor, Channel<T>, TResult> actorMethod);
    }
}

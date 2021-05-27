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
        public abstract string ActorName { get; }
        public abstract bool IsInActorContext { get; }

        public abstract void Send(Action<TActor> method);
        public abstract Task Call(Action<TActor> method);
        public abstract Task Call(Func<TActor, Task> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, TResult> method);
        public abstract Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method);
        public abstract void SendChannel<T>(ActorChannel<T> channel, Action<TActor, ActorChannel<T>> actorMethod);
        public abstract Task OpenChannel<T>(ActorChannel<T> channel, Action<TActor, ActorChannel<T>> actorMethod);
        public abstract Task<TResult> OpenChannel<T, TResult>(ActorChannel<T> channel, Func<TActor, ActorChannel<T>, TResult> actorMethod);
    }
}

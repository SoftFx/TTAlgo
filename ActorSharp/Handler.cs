using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class Handler
    {
        internal IActorRef ActorRef { get; private set; }

        internal void Init(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }
    }

    public class Handler<TActor> : Handler
        where TActor : Actor
    {
        protected void PostMessage(object message)
        {
            ActorRef.PostMessage(message);
        }

        protected Task CallActor(Action<TActor> method)
        {
            return ActorRef.CallActor(method);
        }

        protected Task<TResult> CallActor<TResult>(Func<TActor, TResult> method)
        {
            return ActorRef.CallActor(method);
        }

        protected Task CallActor(Func<TActor, Task> method)
        {
            return ActorRef.CallActor(method);
        }

        protected Task<TResult> CallActor<TResult>(Func<TActor, Task<TResult>> method)
        {
            return ActorRef.CallActor(method);
        }

        protected IRxChannel<T> Marshal<T>(ITxChannel<T> channel, int pageSize = 10)
        {
            return ActorRef.Marshal(channel, pageSize);
        }

        protected IRxChannel<T> NewRxChannel<T>()
        {
            return ActorRef.NewRxChannel<T>();
        }

        protected ITxChannel<T> NewTxChannel<T>()
        {
            return ActorRef.NewTxChannel<T>();
        }
    }
}

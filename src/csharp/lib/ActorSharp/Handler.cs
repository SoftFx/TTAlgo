using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class Handler<TActor> : ActorPart
        where TActor : Actor
    {
        protected Ref<TActor> Actor { get; }

        public Handler(Ref<TActor> actorRef)
        {
            Actor = actorRef ?? throw new ArgumentNullException("actorRef");
        }
    }

    public class BlockingHandler<TActor>
    {
        protected Ref<TActor> Actor { get; }

        public BlockingHandler(Ref<TActor> actorRef)
        {
            Actor = actorRef ?? throw new ArgumentNullException("actorRef");
        }

        protected void ActorSend(Action<TActor> actorMethod)
        {
            Actor.Send(actorMethod);
        }

        protected void CallActor(Action<TActor> actorMethod)
        {
            Actor.Call(actorMethod).Wait();
        }

        protected TResult CallActor<TResult>(Func<TActor, TResult> actorMethod)
        {
            return Actor.Call(actorMethod).Result;
        }

        protected void CallActor(Func<TActor, Task> actorMethod)
        {
            Actor.Call(actorMethod).Wait();
        }

        protected Task CallActorAsync(Action<TActor> actorMethod)
        {
            return Actor.Call(actorMethod);
        }

        protected Task<TResult> CallActorAsync<TResult>(Func<TActor, TResult> actorMethod)
        {
            return Actor.Call(actorMethod);
        }

        protected Task CallActorAsync(Func<TActor, Task> actorMethod)
        {
            return Actor.Call(actorMethod);
        }

        protected TResult CallActor<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            return Actor.Call(actorMethod).Result;
        }

        protected Task CallActorAsyncMethod(Func<TActor, Task> actorMethod)
        {
            return Actor.Call(actorMethod);
        }

        protected Task<TResult> CallActorAsyncMethod<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            return Actor.Call(actorMethod);
        }

        protected void CallActorFlatten(Action<TActor> actorMethod)
        {
            try
            {
                Actor.Call(actorMethod).Wait();
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected TResult CallActorFlatten<TResult>(Func<TActor, TResult> actorMethod)
        {
            try
            {
                return Actor.Call(actorMethod).Result;
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected void CallActorFlatten(Func<TActor, Task> actorMethod)
        {
            try
            {
                Actor.Call(actorMethod).Wait();
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected TResult CallActorFlatten<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            try
            {
                return Actor.Call(actorMethod).Result;
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected async Task CallActorFlattenAsync(Func<TActor, Task> actorMethod)
        {
            try
            {
                await Actor.Call(actorMethod);
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected async Task<TResult> CallActorFlattenAsync<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            try
            {
                return await Actor.Call(actorMethod);
            }
            catch (AggregateException aex)
            {
                throw FlattenAsPossible(aex);
            }
        }

        protected BlockingChannel<T> OpenInputChannel<T>(int pageSize, Action<TActor, ActorChannel<T>> actorMethod)
        {
            return Actor.OpenBlockingChannel(ChannelDirections.In, pageSize, actorMethod);
        }

        protected BlockingChannel<T> OpenOutputChannel<T>(Action<TActor, ActorChannel<T>> actorMethod)
        {
            return Actor.OpenBlockingChannel(ChannelDirections.Out, 10, actorMethod);
        }

        private Exception FlattenAsPossible(Exception ex)
        {
            var aggrEx = ex as AggregateException;
            if (aggrEx != null && aggrEx.InnerExceptions.Count == 1)
                return FlattenAsPossible(aggrEx.InnerExceptions[0]);
            else
                return ex;
        }
    }
}

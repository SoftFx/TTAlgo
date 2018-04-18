using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ActorSharp
{
    public static class ActorExtentions
    {
        public static Ref<TActor> GetRef<TActor>(this TActor actor)
            where TActor : Actor
        {
            #if DEBUG
            if (SynchronizationContext.Current != actor.Context)
                throw new InvalidOperationException("Synchronization violation! You cannot create reference from outside the actor context! Only the actor can create reference to itself!");
            #endif

            return new LocalRef<TActor>(actor, actor.Context);
        }

        public static Ref<TActorBase> Cast<TActor, TActorBase>(this Ref<TActor> actorRef)
            where TActor : TActorBase
        {
            var localRef = (LocalRef<TActor>)actorRef;
            return new LocalRef<TActorBase>(localRef.ActorInstance, localRef.ActorContext);
        }

        public static IAsyncReader<TResult> Select<TSrc, TResult>(this IAsyncReader<TSrc> srcEnumerable, Func<TSrc, TResult> selector)
        {
            return new AsyncSelect<TSrc, TResult>(srcEnumerable, selector);
        }

        public static BlockingChannel<TData> OpenBlockingChannel<TActor, TData>(this Ref<TActor> actor, ChannelDirections direction, int pageSize, Action<TActor, Channel<TData>> actorMethod)
        {
            var callTask = actor.Call(a =>
            {
                var actorSide = new Channel<TData>(direction, pageSize);
                var handlerSide = new BlockingChannel<TData>(actorSide);
                actorMethod(a, actorSide);
                return handlerSide;
            });

            return callTask.Result;
        }

        public static async void WriteAll<TData>(this Channel<TData> channel, Func<IEnumerable<TData>> enumerableFactory)
        {
            try
            {
                var e = enumerableFactory();

                foreach (var item in e)
                    await channel.Write(item);

                await channel.Close();
            }
            catch (Exception ex)
            {
                await channel.Close(ex);
            }
        }

        private class AsyncSelect<TSrc, TResult> : IAsyncReader<TResult>
        {
            private IAsyncReader<TSrc> _src;
            private Func<TSrc, TResult> _selector;

            public AsyncSelect(IAsyncReader<TSrc> src, Func<TSrc, TResult> selector)
            {
                _src = src ?? throw new ArgumentNullException("src");
                _selector = selector;
            }

            public TResult Current => _selector(_src.Current);

            public IAwaitable<bool> ReadNext()
            {
                return _src.ReadNext();
            }
        }
    }
}

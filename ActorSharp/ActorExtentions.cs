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
    }
}

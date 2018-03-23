﻿using System;
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

        protected TResult CallActor<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            return Actor.Call(actorMethod).Result;
        }

        protected BlockingChannel<T> OpenInputChannel<T>(int pageSize, Action<TActor, Channel<T>> actorMethod)
        {
            return Actor.OpenBlockingChannel(ChannelDirections.In, pageSize, actorMethod);
        }

        protected BlockingChannel<T> OpenOutputChannel<T>(Action<TActor, Channel<T>> actorMethod)
        {
            return Actor.OpenBlockingChannel(ChannelDirections.Out, 10, actorMethod);
        }
    }
}

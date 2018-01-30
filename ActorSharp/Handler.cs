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
        private Ref<TActor> Ref { get; }

        public BlockingHandler(Ref<TActor> actorRef)
        {
            Ref = actorRef ?? throw new ArgumentNullException("actorRef");
        }

        protected void CallActor(Action<TActor> actorMethod)
        {
            Ref.Call(actorMethod).Wait();
        }

        protected TResult CallActor<TResult>(Func<TActor, TResult> actorMethod)
        {
            return Ref.Call(actorMethod).Result;
        }

        protected TResult CallActor<TResult>(Func<TActor, Task<TResult>> actorMethod)
        {
            return Ref.Call(actorMethod).Result;
        }

        protected BlockingChannel<T> OpenInputChannel<T>(int pageSize, Action<TActor, Channel<T>> actorMethod)
        {
            var callTask = Ref.Call(a =>
            {
                var actorSide = new Channel<T>(ChannelDirections.In, pageSize);
                var handlerSide = new BlockingChannel<T>(actorSide);
                actorMethod(a, actorSide);
                return handlerSide;
            });

            return callTask.Result;
        }
    }
}

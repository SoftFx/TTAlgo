using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp
{
    public class ControlHandler<TActor> : Handler<TActor>
        where TActor : Actor, new()
    {
        public ControlHandler()
        {
            Init(Actor.SpawnLocal<TActor>());
        }

        public ControlHandler(IActorFactory factory)
        {
            Init(factory.Spawn<TActor>());
        }

        protected Ref<THandler> GetHandlerRef<THandler>()
            where THandler : Handler<TActor>, new()
        {
            return new Ref<THandler>(ActorRef);
        }
    }
}

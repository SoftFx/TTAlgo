using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp
{
    public class Ref<THandler>
        where THandler : Handler, new()
    {
        private IActorRef _actorRef;

        internal Ref(IActorRef actorRef)
        {
            _actorRef = actorRef ?? throw new ArgumentNullException("actorRef");
        }

        public THandler CreateHandler()
        {
            var h = new THandler();
            h.Init(_actorRef);
            return h;
        }
    }
}

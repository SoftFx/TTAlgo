using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ActorSharp
{
    public class ActorPart : Actor
    {
        public ActorPart()
        {
            Context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
            ActorInit();
        }

        //internal ActorPart(SynchronizationContext context)
        //{
        //    Context = context ?? throw new Exception("Synchronization context is required!");
        //    ActorInit();
        //}
    }
}

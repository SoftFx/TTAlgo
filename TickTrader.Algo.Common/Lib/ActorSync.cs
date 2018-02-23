using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.Algo.Common.Lib
{
    /// <summary>
    /// Warning! This violates Actor Model rules. This is temporary and should be removed!
    /// </summary>
    public static class ActorSync
    {
        public static ISyncContext Create<TActor>(Ref<TActor> target)
            where TActor : Actor
        {
            return new Adapter<TActor>(target);
        }

        public static ISyncContext GetSyncContext<TActor>(this Ref<TActor> target)
            where TActor : Actor
        {
            return Create(target);
        }

        public static ISyncContext GetSyncContext<TActor>(this TActor actor)
            where TActor : Actor
        {
            return new Adapter<TActor>(actor.GetRef());
        }

        private class Adapter<TActor> : BlockingHandler<TActor>, ISyncContext
        {
            public Adapter(Ref<TActor> aRef) : base(aRef) { }

            public void Invoke(Action syncAction)
                => CallActor(a => syncAction());

            public void Invoke<T>(Action<T> syncAction, T args)
                => CallActor(a => syncAction(args));

            public T Invoke<T>(Func<T> syncFunc)
                => CallActor(a => syncFunc());

            public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
                => CallActor(a => syncFunc(args));
        }
    }
}

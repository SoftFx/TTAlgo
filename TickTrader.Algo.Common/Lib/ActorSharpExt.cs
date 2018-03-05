using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Lib
{
    public static class ActorSharpExt
    {
        public static IAsyncCrossDomainEnumerator<T> AsCrossDomain<T>(this BlockingChannel<T> channel)
            where T : class
        {
            return new CrossDomainAdapter<T>(channel);
        }

        public static IAsyncCrossDomainEnumerator<TOut> AsCrossDomain<TIn, TOut>(this BlockingChannel<TIn> channel, Func<TIn, TOut> selector)
            where TOut : class
        {
            return new CrossDomainSelectAdapter<TIn, TOut>(channel, selector);
        }

        public static ISyncContext GetSyncContext<TActor>(this Ref<TActor> target)
            where TActor : Actor
        {
            return new SyncAdapter<TActor>(target);
        }

        public static ISyncContext GetSyncContext<TActor>(this TActor actor)
            where TActor : Actor
        {
            return new SyncAdapter<TActor>(actor.GetRef());
        }

        private class SyncAdapter<TActor> : BlockingHandler<TActor>, ISyncContext
        {
            public SyncAdapter(Ref<TActor> aRef) : base(aRef) { }

            public void Invoke(Action syncAction)
                => CallActor(a => syncAction());

            public void Invoke<T>(Action<T> syncAction, T args)
                => CallActor(a => syncAction(args));

            public T Invoke<T>(Func<T> syncFunc)
                => CallActor(a => syncFunc());

            public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
                => CallActor(a => syncFunc(args));
        }

        private class CrossDomainAdapter<T> : CrossDomainObject, IAsyncCrossDomainEnumerator<T>
            where T : class
        {
            private BlockingChannel<T> _channel;

            public CrossDomainAdapter(BlockingChannel<T> channel)
            {
                _channel = channel;
            }

            public void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback)
            {
                try
                {
                    pageCallback.SetResult(_channel.ReadPage());
                }
                catch (Exception ex)
                {
                    pageCallback.SetException(ex);
                }
            }
        }

        private class CrossDomainSelectAdapter<TIn, TOut> : CrossDomainObject, IAsyncCrossDomainEnumerator<TOut>
            where TOut : class
        {
            private BlockingChannel<TIn> _channel;
            private Func<TIn, TOut> _selector;

            public CrossDomainSelectAdapter(BlockingChannel<TIn> channel, Func<TIn, TOut> selector)
            {
                _channel = channel;
                _selector = selector;
            }

            public void GetNextPage(CrossDomainTaskProxy<TOut[]> pageCallback)
            {
                try
                {
                    var page = _channel.ReadPage();
                    pageCallback.SetResult(Select(page));
                }
                catch (Exception ex)
                {
                    pageCallback.SetException(ex);
                }
            }

            private TOut[] Select(TIn[] page)
            {
                if (page == null)
                    return null;

                var result = new TOut[page.Length];

                for (int i = 0; i < page.Length; i++)
                    result[i] = _selector(page[i]);

                return result;
            }
        }
    }
}

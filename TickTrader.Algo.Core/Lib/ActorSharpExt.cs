using ActorSharp;
using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class ActorSharpExt
    {
        public static IAsyncPagedEnumerator<T> AsPagedEnumerator<T>(this BlockingChannel<T> channel)
            where T : class
        {
            return new PagedAdapter<T>(channel);
        }

        public static IAsyncPagedEnumerator<TOut> AsPagedEnumerator<TIn, TOut>(this BlockingChannel<TIn> channel, Func<TIn, TOut> selector)
            where TOut : class
        {
            return new PagedSelectAdapter<TIn, TOut>(channel, selector);
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
            {
                if (Actor.IsInActorContext)
                    syncAction();
                else
                    CallActor(a => syncAction());
            }

            public void Invoke<T>(Action<T> syncAction, T args)
            {
                if (Actor.IsInActorContext)
                    syncAction(args);
                else
                    CallActor(a => syncAction(args));
            }

            public T Invoke<T>(Func<T> syncFunc)
            {
                if (Actor.IsInActorContext)
                    return syncFunc();
                else
                    return CallActor(a => syncFunc());
            }

            public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
            {
                if (Actor.IsInActorContext)
                    return syncFunc(args);
                else
                    return CallActor(a => syncFunc(args));
            }

            public void Send(Action asyncAction) => ActorSend(a => asyncAction());
        }

        private class PagedAdapter<T> : IAsyncPagedEnumerator<T>
            where T : class
        {
            private BlockingChannel<T> _channel;

            public PagedAdapter(BlockingChannel<T> channel)
            {
                _channel = channel;
            }

            public Task<T[]> GetNextPage()
            {
                return Task.FromResult(_channel.ReadPage());
            }

            public void Dispose()
            {
                _channel.Close();
            }
        }

        private class PagedSelectAdapter<TIn, TOut> : IAsyncPagedEnumerator<TOut>
            where TOut : class
        {
            private BlockingChannel<TIn> _channel;
            private Func<TIn, TOut> _selector;

            public PagedSelectAdapter(BlockingChannel<TIn> channel, Func<TIn, TOut> selector)
            {
                _channel = channel;
                _selector = selector;
            }

            public Task<TOut[]> GetNextPage()
            {
                var page = _channel.ReadPage();
                return Task.FromResult(Select(page));
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

            public void Dispose()
            {
                _channel.Close();
            }
        }
    }
}

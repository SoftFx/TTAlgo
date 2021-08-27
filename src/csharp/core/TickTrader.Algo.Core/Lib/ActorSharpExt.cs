using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class ActorSharpExt
    {
        public static IAsyncPagedEnumerator<T> AsPagedEnumerator<T>(this Channel<T> channel, int pageSize = 1000)
            where T : class
        {
            return new PagedAdapter<T>(channel, pageSize);
        }

        public static IAsyncPagedEnumerator<TOut> AsPagedEnumerator<TIn, TOut>(this Channel<TIn> channel, Func<TIn, TOut> selector, int pageSize = 1000)
            where TOut : class
        {
            return new PagedSelectAdapter<TIn, TOut>(channel, selector, pageSize);
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
            private Channel<T> _channel;
            private int _pageSize;

            public PagedAdapter(Channel<T> channel, int pageSize)
            {
                _channel = channel;
                _pageSize = pageSize;
            }

            public async Task<List<T>> GetNextPage()
            {
                var reader = _channel.Reader;
                var canRead = await reader.WaitToReadAsync();
                if (!canRead)
                    return null;

                var page = new List<T>(_pageSize);
                while (page.Count < page.Capacity && reader.TryRead(out var item))
                {
                    page.Add(item);
                }
                return page;
            }

            public void Dispose()
            {
                _channel.Writer.TryComplete();
            }
        }

        private class PagedSelectAdapter<TIn, TOut> : IAsyncPagedEnumerator<TOut>
            where TOut : class
        {
            private Channel<TIn> _channel;
            private Func<TIn, TOut> _selector;
            private int _pageSize;

            public PagedSelectAdapter(Channel<TIn> channel, Func<TIn, TOut> selector, int pageSize)
            {
                _channel = channel;
                _selector = selector;
                _pageSize = pageSize;
            }

            public async Task<List<TOut>> GetNextPage()
            {
                var reader = _channel.Reader;
                var canRead = await reader.WaitToReadAsync();
                if (!canRead)
                    return null;

                var page = new List<TOut>(_pageSize);
                while (page.Count < page.Capacity && reader.TryRead(out var item))
                {
                    page.Add(_selector(item));
                }
                return page;
            }

            public void Dispose()
            {
                _channel.Writer.TryComplete();
            }
        }
    }
}

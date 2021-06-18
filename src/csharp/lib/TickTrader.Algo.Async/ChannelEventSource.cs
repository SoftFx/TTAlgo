using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;

namespace TickTrader.Algo.Async
{
    public interface IEventSource<T>
    {
        IDisposable Subscribe(Action<T> handler);

        IDisposable Subscribe(Func<T, Task> handler);

        IDisposable Subscribe(IActorRef actor);

        IDisposable Subscribe<TMsg>(IActorRef actor, Func<T, TMsg> msgBuilder);
    }

    public sealed class ChannelEventSource<T> : IEventSource<T>, IDisposable
    {
        private readonly Channel<T> _channel;
        private readonly List<EventSubscription> _subList = new List<EventSubscription>();
        private readonly object _subLock = new object();
        private readonly CancellationTokenSource _cancelTokenSrc;

        private int _subListVersion = 0;


        public ChannelWriter<T> Writer => _channel.Writer;


        public ChannelEventSource(int batchSize = 10)
        {
            _channel = DefaultChannelFactory.CreateForEvent<T>();

            _cancelTokenSrc = new CancellationTokenSource();
            var _ = DispatchEvents(_channel.Reader, batchSize, _cancelTokenSrc.Token);
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
            _cancelTokenSrc.Cancel();
        }

        public IDisposable Subscribe(Action<T> handler) => new SyncEventSubscription(handler, this);

        public IDisposable Subscribe(Func<T, Task> handler) => new AsyncEventSubscription(handler, this);

        public IDisposable Subscribe(IActorRef actor) => new ActorEventSubscription(actor, this);

        public IDisposable Subscribe<TMsg>(IActorRef actor, Func<T, TMsg> msgBuilder) => new ActorEventSubscription<TMsg>(actor, msgBuilder, this);


        private async Task DispatchEvents(ChannelReader<T> reader, int batchSize, CancellationToken cancelToken)
        {
            await Task.Yield();

            while (!cancelToken.IsCancellationRequested && await reader.WaitToReadAsync())
            {
                EventSubscription[] subListCache;
                int cacheVersion;
                lock (_subLock)
                {
                    subListCache = _subList.ToArray();
                    cacheVersion = _subListVersion;
                }

                var n = subListCache.Length;
                var i = 0;
                for (; i < batchSize; i++)
                {
                    if (cancelToken.IsCancellationRequested || !reader.TryRead(out var item))
                        break;

                    for (var j = 0; j < n; j++)
                    {
                        try
                        {
                            var t = subListCache[j].DispatchEvent(item);
                            if (t != null)
                                await t;
                        }
                        catch (Exception) { }
                    }

                    if (_subListVersion != cacheVersion)
                        break;
                }

                await Task.Yield(); // break sync processing
            }
        }

        private void AddSub(EventSubscription sub)
        {
            lock (_subLock)
            {
                _subListVersion++;
                _subList.Add(sub);
            }
        }

        private void RemoveSub(EventSubscription sub)
        {
            lock (_subLock)
            {
                _subListVersion++;
                _subList.Remove(sub);
            }
        }


        private class EventSubscription : IDisposable
        {
            private readonly ChannelEventSource<T> _parent;


            protected EventSubscription(ChannelEventSource<T> parent)
            {
                _parent = parent;
                parent.AddSub(this);
            }


            public void Dispose()
            {
                _parent.RemoveSub(this);
            }

            public virtual Task DispatchEvent(T args) => null;
        }

        private sealed class SyncEventSubscription : EventSubscription
        {
            private readonly Action<T> _handler;


            public SyncEventSubscription(Action<T> handler, ChannelEventSource<T> parent)
                : base(parent)
            {
                _handler = handler;
            }


            public override Task DispatchEvent(T args)
            {
                _handler(args);
                return null;
            }
        }

        private sealed class AsyncEventSubscription : EventSubscription
        {
            private readonly Func<T, Task> _handler;


            public AsyncEventSubscription(Func<T, Task> handler, ChannelEventSource<T> parent)
                : base(parent)
            {
                _handler = handler;
            }


            public override Task DispatchEvent(T args)
            {
                return _handler(args);
            }
        }

        private sealed class ActorEventSubscription : EventSubscription
        {
            private readonly IActorRef _actor;


            public ActorEventSubscription(IActorRef actor, ChannelEventSource<T> parent)
                : base(parent)
            {
                _actor = actor;
            }


            public override Task DispatchEvent(T args)
            {
                _actor.Tell(args);
                return null;
            }
        }

        private sealed class ActorEventSubscription<TMsg> : EventSubscription
        {
            private readonly IActorRef _actor;
            private readonly Func<T, TMsg> _msgBuilder;


            public ActorEventSubscription(IActorRef actor, Func<T, TMsg> msgBuilder, ChannelEventSource<T> parent)
                : base(parent)
            {
                _actor = actor;
                _msgBuilder = msgBuilder;
            }


            public override Task DispatchEvent(T args)
            {
                _actor.Tell(_msgBuilder(args));
                return null;
            }
        }
    }
}

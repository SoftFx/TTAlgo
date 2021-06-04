using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async
{
    public interface IEventSource<T>
    {
        IDisposable Subscribe(Action<T> handler);

        IDisposable Subscribe(Func<T, Task> handler);
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

        public IDisposable Subscribe(Action<T> handler)
        {
            var sub = new EventSubscription(handler, RemoveSub);
            AddSub(sub);
            return sub;
        }

        public IDisposable Subscribe(Func<T, Task> handler)
        {
            var sub = new EventSubscription(handler, RemoveSub);
            AddSub(sub);
            return sub;
        }


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
                            await subListCache[j].DispatchEvent(item);
                        }
                        catch (Exception) { }
                    }

                    if (_subListVersion != cacheVersion)
                        break;
                }

                if (i == batchSize)
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


        private sealed class EventSubscription : IDisposable
        {
            private readonly Action<T> _syncHandler;
            private readonly Func<T, Task> _asyncHandler;
            private readonly Action<EventSubscription> _removeSub;


            public EventSubscription(Action<T> handler, Action<EventSubscription> removeSub)
                : this(removeSub)
            {
                _syncHandler = handler;
            }

            public EventSubscription(Func<T, Task> handler, Action<EventSubscription> removeSub)
                : this(removeSub)
            {
                _asyncHandler = handler;
            }

            private EventSubscription(Action<EventSubscription> removeSub)
            {
                _removeSub = removeSub;
            }


            public void Dispose()
            {
                _removeSub(this);
            }

            public async ValueTask DispatchEvent(T args)
            {
                if (_syncHandler != null)
                    _syncHandler(args);
                else await _asyncHandler(args);
            }
        }
    }
}

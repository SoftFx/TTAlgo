﻿using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async
{
    public class ChannelConsumerWrapper<T> : IDisposable
    {
        private readonly object _syncObj = new object();
        private readonly Channel<T> _channel;
        private CancellationTokenSource _cancelTokenSrc;
        private Task _consumeTask;


        public string Name { get; }

        public int ItemsCount => _channel.Reader.Count;

        public int WorkersCnt { get; set; } = 1;

        public int BatchSize { get; set; } = 1;


        public ChannelConsumerWrapper(Channel<T> channel, string name)
        {
            _channel = channel;
            Name = name;
        }


        public override string ToString()
        {
            return $"{Name}: {ItemsCount} items";
        }

        public void Dispose()
        {
            lock (_syncObj)
            {
                if (_cancelTokenSrc == null)
                    return;

                _cancelTokenSrc.Cancel();
                _cancelTokenSrc = null;
                _channel.Writer.TryComplete();
            }
        }


        public void Start(Func<T, Task> itemAction)
        {
            if (itemAction == null)
                throw new ArgumentNullException(nameof(itemAction));

            CancellationToken cancelToken;
            lock (_syncObj)
            {
                if (_cancelTokenSrc != null)
                    throw new Exception($"{Name} already started!");

                _cancelTokenSrc = new CancellationTokenSource();
                cancelToken = _cancelTokenSrc.Token;
            }

            _consumeTask = _channel.Consume(itemAction, BatchSize, WorkersCnt, cancelToken);
        }

        public void Start(Action<T> itemAction)
        {
            if (itemAction == null)
                throw new ArgumentNullException(nameof(itemAction));

            CancellationToken cancelToken;
            lock (_syncObj)
            {
                if (_cancelTokenSrc != null)
                    throw new Exception($"{Name} already started!");

                _cancelTokenSrc = new CancellationTokenSource();
                cancelToken = _cancelTokenSrc.Token;
            }

            _consumeTask = _channel.Consume(itemAction, BatchSize, WorkersCnt, cancelToken);
        }

        public async Task Stop()
        {
            Dispose();

            await _consumeTask;
        }

        public void Add(T item) => _channel.Writer.TryWrite(item);
    }
}

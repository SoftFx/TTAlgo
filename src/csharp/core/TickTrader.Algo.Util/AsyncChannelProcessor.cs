using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Util
{
    public interface IAsyncChannel<T>
    {
        ValueTask AddAsync(T item);

        ValueTask AddAsync(T[] items);

        ValueTask AddAsync(IList<T> items);
    }


    public class AsyncChannelProcessor<T> : IAsyncChannel<T>
    {
        private readonly Channel<T> _channel;
        private int _cnt;
        private bool _isStarted, _isStopPending, _doProcessing;
        private Task[] _workers;


        public string Name { get; }

        public int ItemsCount => _cnt;

        public int WorkersCnt { get; set; } = 1;

        public int BatchSize { get; set; } = 1;


        public AsyncChannelProcessor(Channel<T> channel, string name)
        {
            _channel = channel;
            Name = name;

            _cnt = 0;
        }


        public static AsyncChannelProcessor<T> Create(ChannelOptions options, string name)
        {
            var channel = options is BoundedChannelOptions
                ? Channel.CreateBounded<T>((BoundedChannelOptions)options)
                : Channel.CreateUnbounded<T>((UnboundedChannelOptions)options);
            return new AsyncChannelProcessor<T>(channel, name);
        }

        public static AsyncChannelProcessor<T> CreateUnbounded(string name, bool singleReader = false)
        {
            var options = new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = singleReader,
                SingleWriter = false,
            };
            return new AsyncChannelProcessor<T>(Channel.CreateUnbounded<T>(options), name);
        }


        public override string ToString()
        {
            return $"{Name}: {_cnt} items";
        }


        public void Start(Func<T, Task> itemAction)
        {
            if (itemAction == null)
                throw new ArgumentNullException(nameof(itemAction));
            if (_isStarted)
                throw new Exception($"{Name} already started!");

            _isStarted = true;
            _doProcessing = true;
            _workers = new Task[WorkersCnt];
            for (var i = 0; i < _workers.Length; i++)
                _workers[i] = ProcessItems(itemAction);
        }

        public async Task Stop(bool finish = true)
        {
            if (!_isStarted)
                throw new Exception($"{Name} is not started!");

            _isStopPending = true;
            _doProcessing = finish;
            _channel.Writer.Complete();

            try
            {
                await Task.WhenAll(_workers);
            }
            finally
            {
                _isStopPending = false;
                _isStarted = false;
            }
        }


        public void Add(T item)
        {
            if (!EnqueueInternal(_channel.Writer, item) && !_isStopPending)
                    throw new Exception($"Channel {Name} failed to write item");
        }

        public async ValueTask AddAsync(T item)
        {
            var writer = _channel.Writer;
            while (!EnqueueInternal(writer, item))
                if (!await writer.WaitToWriteAsync())
                    return;
        }

        public async ValueTask AddAsync(T[] items)
        {
            var writer = _channel.Writer;

            for (var i = 0; i < items.Length; i++)
                while (!EnqueueInternal(writer, items[i]))
                    if (!await writer.WaitToWriteAsync())
                        return;
        }

        public async ValueTask AddAsync(IList<T> items)
        {
            var writer = _channel.Writer;

            for (var i = 0; i < items.Count; i++)
                while (!EnqueueInternal(writer, items[i]))
                    if (!await writer.WaitToWriteAsync())
                        return;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnqueueInternal(ChannelWriter<T> writer, T item)
        {
            var res = writer.TryWrite(item);
            if (res)
                Interlocked.Increment(ref _cnt);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DequeueInternal(ChannelReader<T> reader, out T item)
        {
            var res = reader.TryRead(out item);
            if (res)
                Interlocked.Decrement(ref _cnt);
            return res;
        }

        private async Task ProcessItems(Func<T, Task> itemAction)
        {
            await Task.Yield();

            var reader = _channel.Reader;
            while (_doProcessing && await reader.WaitToReadAsync())
            {
                var i = 0;
                for (; i < BatchSize; i++)
                {
                    if (!DequeueInternal(reader, out var item))
                        break;

                    try
                    {
                        await itemAction.Invoke(item);
                    }
                    catch (Exception) { }
                }

                if (i == BatchSize)
                    await Task.Yield(); // break sync processing
            }
        }
    }
}

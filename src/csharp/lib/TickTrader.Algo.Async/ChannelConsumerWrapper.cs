using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async
{
    public class ChannelConsumerWrapper<T>
    {
        private readonly object _syncObj = new object();
        private readonly ChannelOptions _options;
        private Channel<T> _channel;
        private CancellationTokenSource _cancelTokenSrc;
        private Task _consumeTask;


        public string Name { get; }

        public int ItemsCount => _channel.Reader.Count;

        public int WorkersCnt { get; set; } = 1;

        public int BatchSize { get; set; } = 1;


        public ChannelConsumerWrapper(ChannelOptions options, string name)
        {
            _options = options;
            Name = name;

            InitChannel();
        }


        public static ChannelConsumerWrapper<T> CreateUnbounded(string name, bool singleReader = false)
        {
            var options = new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = singleReader,
                SingleWriter = false,
            };
            return new ChannelConsumerWrapper<T>(options, name);
        }


        public override string ToString()
        {
            return $"{Name}: {ItemsCount} items";
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
            lock (_syncObj)
            {
                if (_cancelTokenSrc == null)
                    return;

                _cancelTokenSrc.Cancel();
                _cancelTokenSrc = null;
                _channel.Writer.Complete();
            }

            try
            {
                await _consumeTask;
            }
            finally
            {
                InitChannel();
            }
        }

        public void Add(T item)
        {
            if (!_channel.Writer.TryWrite(item) && !_cancelTokenSrc.IsCancellationRequested)
                throw new Exception($"Channel {Name} failed to write item");
        }


        private void InitChannel()
        {
            lock (_syncObj)
            {
                var options = _options;
                _channel = options is BoundedChannelOptions
                    ? Channel.CreateBounded<T>((BoundedChannelOptions)options)
                    : Channel.CreateUnbounded<T>((UnboundedChannelOptions)options);
            }
        }
    }
}

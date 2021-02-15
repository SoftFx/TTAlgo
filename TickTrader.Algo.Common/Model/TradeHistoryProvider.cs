using ActorSharp;
using ActorSharp.Lib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeHistoryProvider : ActorPart
    {
        private IAlgoCoreLogger logger;

        private ConnectionModel _connection;
        private AsyncLock _updateLock = new AsyncLock();
        private AsyncQueue<TradeReportEntity> _updateQueue;
        private Dictionary<Ref<Handler>, Channel<TradeReportEntity>> _listeners = new Dictionary<Ref<Handler>, Channel<TradeReportEntity>>();
        private bool _isStarted;

        public TradeHistoryProvider(ConnectionModel connection, int loggerId)
        {
            logger = CoreLoggerFactory.GetLogger<TradeHistoryProvider>(loggerId);

            _connection = connection;
            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<TradeReportEntity>();

                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
            };

            _connection.AsyncInitalizing += (s, c) => Start();
            _connection.AsyncDisconnected += (s, c) =>
            {
                _connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;

                return Stop();
            };
        }

        private void TradeProxy_TradeTransactionReport(TradeReportEntity report)
        {
            ContextSend(() => _updateQueue.Enqueue(report));
        }

        private async void GetTradeHistory(Channel<TradeReportEntity> txChannel, DateTime? from, DateTime? to, bool skipCanceledOrders, bool backwards)
        {
            try
            {
                if (!_isStarted)
                    throw new InvalidOperationException("No connection!");

                if (from != null || to != null)
                {
                    from = from ?? new DateTime(1870, 0, 0);
                    to = to ?? DateTime.UtcNow + TimeSpan.FromDays(2);
                }

                var rxChannel = Channel.NewInput<TradeReportEntity>(1000);
                _connection.TradeProxy.GetTradeHistory(CreateBlockingChannel(rxChannel), from, to, skipCanceledOrders, backwards);

                while (await rxChannel.ReadNext())
                {
                    if (!await txChannel.Write(rxChannel.Current))
                    {
                        await rxChannel.Close();
                        return;
                    }
                }

                await txChannel.Close();
            }
            catch (Exception ex)
            {
                await txChannel.Close(ex);
            }
        }


        private Task Start()
        {
            _isStarted = true;

            UpdateLoop();

            logger.Debug("Started.");

            return Task.FromResult(this);
        }

        private async Task Stop()
        {
            logger.Debug("Stopping...");

            _updateQueue.Close();

            logger.Debug("Queue is closed.");

            using (await _updateLock.GetLock("stop")) { }; // wait till update loop is stopped
            _updateQueue = null;

            _isStarted = false;

            logger.Debug("Stopped.");
        }

        private async void UpdateLoop()
        {
            logger.Debug("UpdateLoop() enter");

            using (await _updateLock.GetLock("loop"))
            {
                while (await _updateQueue.Dequeue())
                {
                    var update = _updateQueue.Item;

                    foreach (var channel in _listeners.Values)
                        await channel.Write(update);
                }

                logger.Debug("UpdateLoop() stopped, flushing...");

                foreach (var channel in _listeners.Values) // flush all channels
                    await channel.ConfirmRead();
            }

            logger.Debug("UpdateLoop() exit");
        }

        public class Handler : Handler<TradeHistoryProvider>
        {
            private Ref<Handler> _ref;

            public Handler(Ref<TradeHistoryProvider> actorRef) : base(actorRef)
            {
            }

            public ITradeHistoryProvider AlgoAdapter { get; private set; }

            protected override void ActorInit()
            {
                _ref = this.GetRef();
            }

            internal async Task Init()
            {
                AlgoAdapter = new CrossDomainAapter(Actor);
                var reportStream = Channel.NewOutput<TradeReportEntity>(1000);
                await Actor.OpenChannel(reportStream, (a, c) => a._listeners.Add(_ref, c));
                ReadUpdatesLoop(reportStream);
            }

            public event Action<TradeReportEntity> OnTradeReport;

            public Channel<TradeReportEntity> GetTradeHistory(bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, null, skipCancelOrders);
            }

            public Channel<TradeReportEntity> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(from, to, skipCancelOrders);
            }

            public Channel<TradeReportEntity> GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, to, skipCancelOrders);
            }

            private Channel<TradeReportEntity> GetTradeHistoryInternal(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                var channel = Channel.NewOutput<TradeReportEntity>(1000);
                Actor.OpenChannel(channel, (a, c) => a.GetTradeHistory(c, from, to, skipCancelOrders, true));
                return channel;
            }

            private async void ReadUpdatesLoop(Channel<TradeReportEntity> updateStream)
            {
                while (await updateStream.ReadNext())
                    OnTradeReport?.Invoke(updateStream.Current);
            }
        }

        private class CrossDomainAapter : CrossDomainObject, ITradeHistoryProvider
        {
            private Ref<TradeHistoryProvider> _ref;

            public CrossDomainAapter(Ref<TradeHistoryProvider> historyRef)
            {
                _ref = historyRef;
            }

            IAsyncCrossDomainEnumerator<TradeReportEntity> ITradeHistoryProvider.GetTradeHistory(ThQueryOptions options)
            {
                return GetTradeHistoryInternal(null, null, options).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReportEntity> ITradeHistoryProvider.GetTradeHistory(DateTime from, DateTime to, ThQueryOptions options)
            {
                return GetTradeHistoryInternal(from, to, options).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReportEntity> ITradeHistoryProvider.GetTradeHistory(DateTime to, ThQueryOptions options)
            {
                return GetTradeHistoryInternal(null, to, options).AsCrossDomain();
            }

            private BlockingChannel<TradeReportEntity> GetTradeHistoryInternal(DateTime? from, DateTime? to, ThQueryOptions options)
            {
                bool skipCancels = options.HasFlag(ThQueryOptions.SkipCanceled);
                bool backwards = options.HasFlag(ThQueryOptions.Backwards);

                return _ref.OpenBlockingChannel<TradeHistoryProvider, TradeReportEntity>(ChannelDirections.Out, 1000,
                    (a, c) => a.GetTradeHistory(c, from, to, skipCancels, backwards));
            }
        }
    }
}

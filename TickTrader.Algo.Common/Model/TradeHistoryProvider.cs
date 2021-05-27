using ActorSharp;
using ActorSharp.Lib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeHistoryProvider : ActorPart
    {
        private IAlgoLogger logger;

        private ConnectionModel _connection;
        private AsyncLock _updateLock = new AsyncLock();
        private AsyncQueue<Domain.TradeReportInfo> _updateQueue;
        private Dictionary<Ref<Handler>, ActorChannel<Domain.TradeReportInfo>> _listeners = new Dictionary<Ref<Handler>, ActorChannel<Domain.TradeReportInfo>>();
        private bool _isStarted;

        public TradeHistoryProvider(ConnectionModel connection, int loggerId)
        {
            logger = AlgoLoggerFactory.GetLogger<TradeHistoryProvider>(loggerId);

            _connection = connection;
            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<Domain.TradeReportInfo>();

                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
            };

            _connection.AsyncInitalizing += (s, c) => Start();
            _connection.AsyncDisconnected += (s, c) =>
            {
                _connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;

                return Stop();
            };
        }

        private void TradeProxy_TradeTransactionReport(Domain.TradeReportInfo report)
        {
            ContextSend(() => _updateQueue.Enqueue(report));
        }

        private async void GetTradeHistory(ActorChannel<Domain.TradeReportInfo> txChannel, DateTime? from, DateTime? to, bool skipCanceledOrders, bool backwards)
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

                var rxChannel = ActorChannel.NewInput<Domain.TradeReportInfo>(1000);
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
                AlgoAdapter = new PagedEnumeratorAdapter(Actor);
                var reportStream = ActorChannel.NewOutput<Domain.TradeReportInfo>(1000);
                await Actor.OpenChannel(reportStream, (a, c) => a._listeners.Add(_ref, c));
                ReadUpdatesLoop(reportStream);
            }

            public event Action<Domain.TradeReportInfo> OnTradeReport;

            public ActorChannel<Domain.TradeReportInfo> GetTradeHistory(bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, null, skipCancelOrders);
            }

            public ActorChannel<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(from, to, skipCancelOrders);
            }

            public ActorChannel<Domain.TradeReportInfo> GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, to, skipCancelOrders);
            }

            private ActorChannel<Domain.TradeReportInfo> GetTradeHistoryInternal(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                var channel = ActorChannel.NewOutput<Domain.TradeReportInfo>(1000);
                Actor.OpenChannel(channel, (a, c) => a.GetTradeHistory(c, from, to, skipCancelOrders, true));
                return channel;
            }

            private async void ReadUpdatesLoop(ActorChannel<Domain.TradeReportInfo> updateStream)
            {
                while (await updateStream.ReadNext())
                    OnTradeReport?.Invoke(updateStream.Current);
            }
        }

        private class PagedEnumeratorAdapter : ITradeHistoryProvider
        {
            private Ref<TradeHistoryProvider> _ref;

            public PagedEnumeratorAdapter(Ref<TradeHistoryProvider> historyRef)
            {
                _ref = historyRef;
            }

            public IAsyncPagedEnumerator<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, Domain.TradeHistoryRequestOptions options)
            {
                bool skipCancels = options.HasFlag(Domain.TradeHistoryRequestOptions.SkipCanceled);
                bool backwards = options.HasFlag(Domain.TradeHistoryRequestOptions.Backwards);

                return _ref.OpenBlockingChannel<TradeHistoryProvider, Domain.TradeReportInfo>(ChannelDirections.Out, 1000,
                    (a, c) => a.GetTradeHistory(c, from, to, skipCancels, backwards)).AsPagedEnumerator();
            }
        }
    }
}

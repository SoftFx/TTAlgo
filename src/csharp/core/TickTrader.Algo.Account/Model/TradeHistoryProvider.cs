using ActorSharp;
using ActorSharp.Lib;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Account
{
    public class TradeHistoryProvider : ActorPart
    {
        private readonly Dictionary<Ref<Handler>, ActorChannel<object>> _listeners = new Dictionary<Ref<Handler>, ActorChannel<object>>();

        private readonly AsyncLock _updateLock = new AsyncLock();
        private readonly ConnectionModel _connection;
        private readonly IAlgoLogger _logger;

        private AsyncQueue<object> _updateQueue;
        private bool _isStarted;

        public TradeHistoryProvider(ConnectionModel connection, string loggerId)
        {
            _logger = AlgoLoggerFactory.GetLogger<TradeHistoryProvider>(loggerId);

            _connection = connection;
            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<object>();

                _connection.TradeProxy.TradeTransactionReport += SendHistoryUpdate;
                _connection.TradeProxy.TriggerTransactionReport += SendHistoryUpdate;
            };

            _connection.AsyncInitalizing += (s, c) => Start();
            _connection.AsyncDisconnected += (s, c) =>
            {
                _connection.TradeProxy.TradeTransactionReport -= SendHistoryUpdate;
                _connection.TradeProxy.TriggerTransactionReport -= SendHistoryUpdate;
                return Stop();
            };
        }

        private void SendHistoryUpdate(object report)
        {
            ContextSend(() => _updateQueue.Enqueue(report));
        }

        private void GetTradeHistory(Channel<Domain.TradeReportInfo> txChannel, DateTime? from, DateTime? to, bool skipCanceledOrders, bool backwards)
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

                _connection.TradeProxy.GetTradeHistory(txChannel.Writer, from, to, skipCanceledOrders, backwards);
            }
            catch (Exception ex)
            {
                txChannel.Writer.TryComplete(ex);
            }
        }

        private void GetTriggerReportsHistory(Channel<Domain.TriggerReportInfo> txChannel, DateTime? from, DateTime? to, bool skipFailedTriggers, bool backwards)
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

                _connection.TradeProxy.GetTriggerReportsHistory(txChannel.Writer, from, to, skipFailedTriggers, backwards);
            }
            catch (Exception ex)
            {
                txChannel.Writer.TryComplete(ex);
            }
        }

        private Task Start()
        {
            _isStarted = true;

            _ = UpdateLoop();

            _logger.Debug("Started.");

            return Task.FromResult(this);
        }

        private async Task Stop()
        {
            _logger.Debug("Stopping...");

            _updateQueue.Close();

            _logger.Debug("Queue is closed.");

            using (await _updateLock.GetLock("stop")) { }; // wait till update loop is stopped
            _updateQueue = null;

            _isStarted = false;

            _logger.Debug("Stopped.");
        }

        private async Task UpdateLoop()
        {
            try
            {
                _logger.Debug("UpdateLoop() enter");

                using (await _updateLock.GetLock("loop"))
                {
                    while (await _updateQueue.Dequeue())
                    {
                        var update = _updateQueue.Item;

                        foreach (var channel in _listeners.Values)
                            await channel.Write(update);
                    }

                    _logger.Debug("UpdateLoop() stopped, flushing...");

                    foreach (var channel in _listeners.Values) // flush all channels
                        await channel.ConfirmRead();
                }

                _logger.Debug("UpdateLoop() exit");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateLoop() failed");
            }
        }

        public class Handler : Handler<TradeHistoryProvider>
        {
            private Ref<Handler> _ref;

            public Handler(Ref<TradeHistoryProvider> actorRef) : base(actorRef)
            {
            }

            public ITradeHistoryProvider AlgoAdapter { get; private set; }

            public event Action<Domain.TradeReportInfo> OnTradeReport;
            public event Action<Domain.TriggerReportInfo> OnTriggerHistoryReport;


            protected override void ActorInit()
            {
                _ref = this.GetRef();
            }

            internal async Task Init()
            {
                AlgoAdapter = new PagedEnumeratorAdapter(Actor);
                var reportStream = ActorChannel.NewOutput<object>(1000);
                await Actor.OpenChannel(reportStream, (a, c) => a._listeners.Add(_ref, c));
                _ = ReadUpdatesLoop(reportStream);
            }

            public Channel<Domain.TradeReportInfo> GetTradeHistory(bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, null, skipCancelOrders);
            }

            public Channel<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(from, to, skipCancelOrders);
            }

            public Channel<Domain.TradeReportInfo> GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                return GetTradeHistoryInternal(null, to, skipCancelOrders);
            }

            private Channel<Domain.TradeReportInfo> GetTradeHistoryInternal(DateTime? from, DateTime? to, bool skipCancelOrders)
            {
                var channel = DefaultChannelFactory.CreateUnbounded<Domain.TradeReportInfo>();
                Actor.Call(a => a.GetTradeHistory(channel, from, to, skipCancelOrders, true));
                return channel;
            }

            public Channel<Domain.TriggerReportInfo> GetTriggerReportsHistory(DateTime? from, DateTime? to, bool skipFailedTriggers)
            {
                return GetTriggerHistoryInternal(from, to, skipFailedTriggers);
            }

            private Channel<Domain.TriggerReportInfo> GetTriggerHistoryInternal(DateTime? from, DateTime? to, bool skipFailedTriggers)
            {
                var channel = DefaultChannelFactory.CreateUnbounded<Domain.TriggerReportInfo>();
                Actor.Call(a => a.GetTriggerReportsHistory(channel, from, to, skipFailedTriggers, true));
                return channel;
            }

            private async Task ReadUpdatesLoop(ActorChannel<object> updateStream)
            {
                while (await updateStream.ReadNext())
                {
                    var update = updateStream.Current;

                    switch (update)
                    {
                        case Domain.TradeReportInfo tradeHistoryUpdate:
                            OnTradeReport?.Invoke(tradeHistoryUpdate);
                            break;

                        case Domain.TriggerReportInfo triggerHistoryUpdate:
                            OnTriggerHistoryReport?.Invoke(triggerHistoryUpdate);
                            break;
                    }
                }
            }
        }

        private class PagedEnumeratorAdapter : ITradeHistoryProvider
        {
            private Ref<TradeHistoryProvider> _ref;

            public PagedEnumeratorAdapter(Ref<TradeHistoryProvider> historyRef)
            {
                _ref = historyRef;
            }

            public IAsyncPagedEnumerator<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, Domain.HistoryRequestOptions options)
            {
                bool skipCancels = options.HasFlag(Domain.HistoryRequestOptions.SkipCanceled);
                bool backwards = options.HasFlag(Domain.HistoryRequestOptions.Backwards);

                var channel = DefaultChannelFactory.CreateUnbounded<Domain.TradeReportInfo>();
                _ref.Call(a => a.GetTradeHistory(channel, from, to, skipCancels, backwards));
                return channel.AsPagedEnumerator();
            }

            public IAsyncPagedEnumerator<Domain.TriggerReportInfo> GetTriggerHistory(DateTime? from, DateTime? to, Domain.HistoryRequestOptions options)
            {
                bool skipFailed = options.HasFlag(Domain.HistoryRequestOptions.SkipFailed);
                bool backwards = options.HasFlag(Domain.HistoryRequestOptions.Backwards);

                var channel = DefaultChannelFactory.CreateUnbounded<Domain.TriggerReportInfo>();
                _ref.Call(a => a.GetTriggerReportsHistory(channel, from, to, skipFailed, backwards));
                return channel.AsPagedEnumerator();
            }
        }
    }
}

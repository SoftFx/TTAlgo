using ActorSharp;
using ActorSharp.Lib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeHistoryProvider : ActorPart
    {
        private ConnectionModel _connection;
        private AsyncLock _updateLock = new AsyncLock();
        private AsyncQueue<TradeReportEntity> _updateQueue;
        private Dictionary<Ref<Handler>, Channel<TradeReportEntity>> _listeners = new Dictionary<Ref<Handler>, Channel<TradeReportEntity>>();
        private bool _isStarted;

        public TradeHistoryProvider(ConnectionModel connection)
        {
            _connection = connection;
            _connection.InitProxies += () =>
            {
                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                _updateQueue = new AsyncQueue<TradeReportEntity>();
                Start();
                _isStarted = true;
            };

            _connection.AsyncDisconnected += (s, c) => Stop();

            _connection.DeinitProxies += () =>
            {
                _isStarted = false;
                _connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
            };
        }

        private void TradeProxy_TradeTransactionReport(TradeReportEntity report)
        {
            _updateQueue.Enqueue(report);
        }

        private async Task GetTradeHistory(Channel<TradeReportEntity> txChannel, DateTime? from, DateTime? to, bool skipCanceledOrders)
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
                _connection.TradeProxy.GetTradeHistory(CreateBlocingChannel(rxChannel), from, to, skipCanceledOrders);

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

        private void Start()
        {
            UpdateLoop();
        }

        private async Task Stop()
        {
            _updateQueue.Close();
            using (await _updateLock.GetLock()) { }; // wait till update loop is stopped
            _updateQueue = null;
        }

        private async void UpdateLoop()
        {
            using (await _updateLock.GetLock())
            {
                while (await _updateQueue.Dequeue())
                {
                    var update = _updateQueue.Item;

                    foreach (var channel in _listeners.Values)
                        await channel.Write(update);
                }

                foreach (var channel in _listeners.Values) // flush all channels
                    await channel.ConfirmRead();
            }
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
                AlgoAdapter = new AlgoHistoryProvider(); 
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
                Actor.OpenChannel(channel, (a, c) => a.GetTradeHistory(c, from, to, skipCancelOrders));
                return channel;
            }

            private async void ReadUpdatesLoop(Channel<TradeReportEntity> updateStream)
            {
                while (await updateStream.ReadNext())
                    OnTradeReport?.Invoke(updateStream.Current);
            }
        }

        private class AlgoHistoryProvider : CrossDomainObject, ITradeHistoryProvider
        {
            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(bool skipCancelOrders)
            {
                throw new NotImplementedException();
                //return GetTradeHistory(skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(DateTime from, DateTime to, bool skipCancelOrders)
            {
                throw new NotImplementedException();
                //return GetTradeHistory(from, to, skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }

            IAsyncCrossDomainEnumerator<TradeReport> ITradeHistoryProvider.GetTradeHistory(DateTime to, bool skipCancelOrders)
            {
                throw new NotImplementedException();
                //return GetTradeHistory(to, skipCancelOrders).Select(r => (TradeReport[])r).AsCrossDomain();
            }
        }
    }
}

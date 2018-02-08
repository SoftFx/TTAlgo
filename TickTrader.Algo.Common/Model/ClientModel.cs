using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ClientModel : ActorPart, IQuoteDistributorSource
    {
        protected static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("ClientModel");

        private ConnectionModel _connection;
        private FeedHistoryProviderModel _feedHistory;
        private TradeHistoryProvider _tradeHistory;
        private PluginTradeApiProvider _tradeApi;
        private EntityCache _cache = new EntityCache(AccountModelOptions.None);
        private Dictionary<ActorRef, Channel<EntityCacheUpdate>> _tradeListeners = new Dictionary<ActorRef, Channel<EntityCacheUpdate>>();
        private Dictionary<ActorRef, Channel<QuoteEntity>> _feedListeners = new Dictionary<ActorRef, Channel<QuoteEntity>>();
        private AsyncQueue<object> _updateQueue;
        private AsyncQueue<QuoteEntity> _feedQueue;
        private Ref<ClientModel> _ref;
        private QuoteDistributor _rootDistributor;
        private ActorSharp.Lib.AsyncLock _updateLock = new ActorSharp.Lib.AsyncLock();
        private ActorSharp.Lib.AsyncLock _feedLock = new ActorSharp.Lib.AsyncLock();

        protected override void ActorInit()
        {
            _ref = this.GetRef();
            _rootDistributor = new QuoteDistributor(this);
        }

        private void Init(ConnectionOptions connectionOptions, string historyFolder, FeedHistoryFolderOptions historyOptions)
        {
            _connection = new ConnectionModel(new ConnectionOptions());
            _feedHistory = new FeedHistoryProviderModel(_connection, historyFolder, historyOptions);
            _tradeHistory = new TradeHistoryProvider(_connection);
            _tradeApi = new PluginTradeApiProvider(_connection);

            _tradeApi.OnExclusiveReport += er => _updateQueue.Enqueue(er);

            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<object>();
                _feedQueue = new AsyncQueue<QuoteEntity>();

                MulticastQuotes(); // start multicasting

                _connection.FeedProxy.Tick += FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                _connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;

                //if (_connection.FeedProxy.AutoSymbols)
                //{
                //    _connection.FeedProxy.SymbolInfo += _syncUpdater.OnUpdate;
                //    _connection.FeedProxy.CurrencyInfo += _syncUpdater.OnUpdate;
                //}
            };

            _connection.AsyncInitalizing += (s, c) => Start();

            _connection.AsyncDisconnected += (s, t) =>
            {
                _connection.FeedProxy.Tick += FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                _connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;

                return Stop();

                //if (_connection.FeedProxy.AutoSymbols)
                //{
                //    _connection.FeedProxy.SymbolInfo -= _syncUpdater.OnUpdate;
                //    _connection.FeedProxy.CurrencyInfo -= _syncUpdater.OnUpdate;
            };
        }

        private void FeedProxy_Tick(QuoteEntity q)
        {
            ContextSend(() => _feedQueue.Enqueue(q));
        }

        private void TradeProxy_ExecutionReport(ExecutionReport er)
        {
            ContextSend(() => _updateQueue.Enqueue(er));
        }

        private void TradeProxy_PositionReport(PositionEntity pr)
        {
            ContextSend(() => _updateQueue.Enqueue(pr));
        }

        private void TradeProxy_BalanceOperation(BalanceOperationReport obj)
        {
        }

        private void TradeProxy_TradeTransactionReport(TradeReportEntity obj)
        {
        }

        public class ControlHandler : Data
        {
            public ControlHandler(ConnectionOptions options, string historyFolder, FeedHistoryFolderOptions hsitoryOptions) : base(SpawnLocal<ClientModel>())
            {
                Actor.Send(a => a.Init(options, historyFolder, hsitoryOptions));
            }
        }

        public class Data : Handler<ClientModel>, IQuoteDistributorSource
        {
            public Data(Ref<ClientModel> actorRef, AccountModelOptions options = AccountModelOptions.None) : base(actorRef)
            {
                Cache = new EntityCache(options);
                Distributor = new QuoteDistributor(this);
            }

            public ConnectionModel.Handler Connection { get; private set; }
            public Ref<Data> Ref { get; private set; }
            public EntityCache Cache { get; private set; }
            public FeedHistoryProviderModel.Handler FeedHistory { get; private set; }
            public TradeHistoryProvider.Handler TradeHistory { get; private set; }
            public PluginTradeApiProvider.Handler TradeApi { get; private set; }
            public QuoteDistributor Distributor { get; set; }
            public IVarSet<string, SymbolModel> Symbols => Cache.Symbols;
            public IVarSet<string, CurrencyEntity> Currencies => Cache.Currencies;

            protected override void ActorInit()
            {
                Ref = this.GetRef();
            }

            public async Task Init()
            {
                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.Start();

                FeedHistory = new FeedHistoryProviderModel.Handler(await Actor.Call(a => a._feedHistory.GetRef()));
                await FeedHistory.Init();

                TradeHistory = new TradeHistoryProvider.Handler(await Actor.Call(a => a._tradeHistory.GetRef()));
                await TradeHistory.Init();

                TradeApi = new PluginTradeApiProvider.Handler(await Actor.Call(a => a._tradeApi.GetRef()));

                var updateStream = Channel.NewOutput<EntityCacheUpdate>(1000);
                var snapshot = await Actor.OpenChannel(updateStream, (a, c) => a.AddListener(Ref, c));
                ApplyUpdates(updateStream);

                var quoteStream = Channel.NewOutput<QuoteEntity>(1000);
                await Actor.OpenChannel(quoteStream, (a, c) => a._feedListeners.Add(Ref, c));
                ApplyQuotes(quoteStream);
            }

            public async Task Deinit()
            {
                await Actor.Call(a => a.UnsyncListener(Ref));
                await Actor.Call(a => a._feedListeners.Remove(Ref));
                await Connection.Stop();
            }

            private async void ApplyUpdates(Channel<EntityCacheUpdate> updateStream)
            {
                while (await updateStream.ReadNext())
                    updateStream.Current.Apply(Cache);
            }

            private async void ApplyQuotes(Channel<QuoteEntity> updateStream)
            {
                while (await updateStream.ReadNext())
                {
                    var quote = updateStream.Current;
                    Cache.ApplyQuote(quote);
                    Distributor.UpdateRate(quote);
                }
            }

            void IQuoteDistributorSource.ModifySubscription(string symbol, int depth)
            {
            }
        }

        private async Task Start()
        {
            logger.Debug("Start loading.");

            var tradeApi = _connection.TradeProxy;
            var feedApi = _connection.FeedProxy;

            var getCurrenciesTask = feedApi.GetCurrencies();
            var getSymbolsTask = feedApi.GetSymbols();
            var getInfoTask = tradeApi.GetAccountInfo();

            var currencies = await getCurrenciesTask;

            logger.Debug("Loaded currencies.");

            var symbols = await getSymbolsTask;

            logger.Debug("Loaded symbols.");

            var initFeedTask = LoadQuotesSnapshot(symbols.Select(s => s.Name));

            var accInfo = await getInfoTask;

            logger.Debug("Loaded account info.");

            var accUpdate = new AccountModel.Snapshot(accInfo, new List<OrderEntity>(), new List<PositionEntity>(), accInfo.Assets);
            var snaphost = new EntityCache.Snapshot(symbols, currencies, accUpdate);

            await ApplyUpdate(snaphost);

            await initFeedTask;

            logger.Debug("Loaded quotes snaphsot.");

            var orderStream = Channel.NewInput<OrderEntity>();
            tradeApi.GetTradeRecords(CreateBlocingChannel(orderStream));

            while (await orderStream.ReadNext())
                await ApplyUpdate(new AccountModel.LoadOrderUpdate(orderStream.Current));

            logger.Debug("Loaded orders.");

            await FlushListeners();

            logger.Debug("Done loading.");

            MulticastUpdates();
        }

        private async Task Stop()
        {
            try
            {
                logger.Debug("Stopping...");

                _updateQueue.Close();
                _feedQueue.Close(true);

                using (await _updateLock.GetLock()) { }

                logger.Debug("Stopped update stream.");

                using (await _feedLock.GetLock()) { }

                logger.Debug("Stopped quote stream.");

                await FlushListeners();

                logger.Debug("Done stopping.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            _updateQueue = null;
            _feedQueue = null;
        }

        #region Entity cache

        private EntityCacheUpdate AddListener(ActorRef handleRef, Channel<EntityCacheUpdate> channel)
        {
            _tradeListeners.Add(handleRef, channel);
            return _cache.GetSnapshot();
        }

        private async Task UnsyncListener(ActorRef handlerRef)
        {
            var channel = _tradeListeners.GetOrDefault(handlerRef);
            if (channel != null)
            {
                _tradeListeners.Remove(handlerRef);
                channel.Clear();
                await channel.ConfirmRead();
                await channel.Close();
            }
        }

        private async void MulticastUpdates()
        {
            using (await _updateLock.GetLock())
            {
                while (await _updateQueue.Dequeue())
                {
                    var update = CreateCacheUpdate(_updateQueue.Item);
                    await ApplyUpdate(update);
                }
            }
        }

        private async Task ApplyUpdate(EntityCacheUpdate update)
        {
            if (update != null)
            {
                update.Apply(_cache);

                foreach (var listener in _tradeListeners.Values)
                    await listener.Write(update);
            }
        }

        private async Task FlushListeners()
        {
            foreach (var listener in _tradeListeners.Values)
                await listener.ConfirmRead();
        }

        private EntityCacheUpdate CreateCacheUpdate(object item)
        {
            if (item is ExecutionReport)
                return _cache.Account.GetOrderUpdate((ExecutionReport)item);

            return null;
        }

        #endregion

        #region Quote distribution

        private async Task LoadQuotesSnapshot(IEnumerable<string> allSymbols)
        {
            var groups = _rootDistributor.GetAllSubscriptions(allSymbols)
                .GroupBy(i => i.Item1).ToList();

            foreach (var group in groups)
            {
                var groupSymbols = group.Select(g => g.Item2).ToArray();
                var depth = group.Key;

                await _connection.FeedProxy.SubscribeToQuotes(groupSymbols, depth);
                logger.Debug("Subscribed to " + string.Join(",", groupSymbols));

                var quotes = await _connection.FeedProxy.GetQuoteSnapshot(groupSymbols, depth);
                foreach (var q in quotes)
                    await ApplyQuote(q);
            }
        }

        private async void MulticastQuotes()
        {
            using (await _feedLock.GetLock())
            {
                while (await _feedQueue.Dequeue())
                    await ApplyQuote(_feedQueue.Item);
            }
        }

        private async Task ApplyQuote(QuoteEntity quote)
        {
            _cache.ApplyQuote(quote);
            _rootDistributor.UpdateRate(quote);

            foreach (var listener in _feedListeners.Values)
                await listener.Write(quote);
        }

        void IQuoteDistributorSource.ModifySubscription(string symbol, int depth)
        {
        }

        #endregion
    }
}

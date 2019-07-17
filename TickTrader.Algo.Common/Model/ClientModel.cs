﻿using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ClientModel : Actor, IQuoteDistributorSource
    {
        protected static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("ClientModel");

        private static int ActorNameIdSeed = 1;

        private ConnectionModel _connection;
        private FeedHistoryProviderModel.ControlHandler _feedHistory;
        private TradeHistoryProvider _tradeHistory;
        private PluginTradeApiProvider _tradeApi;
        private EntityCache _cache = new EntityCache();
        private Dictionary<ActorRef, Channel<EntityCacheUpdate>> _tradeListeners = new Dictionary<ActorRef, Channel<EntityCacheUpdate>>();
        private Dictionary<ActorRef, Channel<QuoteEntity>> _feedListeners = new Dictionary<ActorRef, Channel<QuoteEntity>>();
        private AsyncQueue<object> _updateQueue;
        private AsyncQueue<QuoteEntity> _feedQueue;
        private Ref<ClientModel> _ref;
        private QuoteDistributor _rootDistributor;
        private ActorSharp.Lib.AsyncLock _updateLock;
        private ActorSharp.Lib.AsyncLock _feedLock;
        private Dictionary<ActorRef, IFeedSubscription> _feedSubcribers = new Dictionary<ActorRef, IFeedSubscription>();

        protected override void ActorInit()
        {
            _ref = this.GetRef();
            _updateLock = new ActorSharp.Lib.AsyncLock();
            _feedLock = new ActorSharp.Lib.AsyncLock();
            _rootDistributor = new QuoteDistributor(this);
        }

        private void Init(ConnectionOptions connectionOptions, string historyFolder, FeedHistoryFolderOptions historyOptions)
        {
            _connection = new ConnectionModel(connectionOptions);
            _feedHistory = new FeedHistoryProviderModel.ControlHandler(_connection, historyFolder, historyOptions);
            _tradeHistory = new TradeHistoryProvider(_connection);
            _tradeApi = new PluginTradeApiProvider(_connection);

            _tradeApi.OnExclusiveReport += er => _updateQueue.Enqueue(er);

            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<object>();
                _feedQueue = new AsyncQueue<QuoteEntity>();

                _connection.FeedProxy.Tick += FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                _connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                _connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            _connection.AsyncInitalizing += (s, c) => Start();

            _connection.AsyncDisconnected += (s, t) =>
            {
                _connection.FeedProxy.Tick -= FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                _connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
                _connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;

                return Stop();
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

        private void TradeProxy_BalanceOperation(BalanceOperationReport rep)
        {
            ContextSend(() => _updateQueue.Enqueue(rep));
        }

        private void TradeProxy_TradeTransactionReport(TradeReportEntity obj)
        {
        }

        public class ControlHandler : BlockingHandler<ClientModel>
        {
            public ControlHandler(ConnectionOptions options, string historyFolder, FeedHistoryFolderOptions hsitoryOptions)
                : base(SpawnLocal<ClientModel>(null, "ClientModel " + Interlocked.Increment(ref ActorNameIdSeed)))
            {
                ActorSend(a => a.Init(options, historyFolder, hsitoryOptions));
            }

            public Data CreateDataHandler() => new Data(Actor);
        }

        public class ControlHandler2 : Handler<ClientModel>
        {
            public ControlHandler2(ConnectionOptions options, string historyFolder, FeedHistoryFolderOptions hsitoryOptions)
                : base(SpawnLocal<ClientModel>(null, "ClientModel " + Interlocked.Increment(ref ActorNameIdSeed)))
            {
                Actor.Send(a => a.Init(options, historyFolder, hsitoryOptions));
            }

            public ConnectionModel.Handler Connection { get; private set; }

            public async Task OpenHandler()
            {
                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.OpenHandler();
            }

            public async Task CloseHandler()
            {
                await Connection.CloseHandler();
            }

            public Task<List<SymbolEntity>> GetSymbols() => Actor.Call(a => a.ExecDataRequest(c => c.GetSymbolsCopy()));
            public Task<List<CurrencyEntity>> GetCurrecnies() => Actor.Call(a => a.ExecDataRequest(c => c.GetCurrenciesCopy()));
            public Task<AccountTypes> GetAccountType() => Actor.Call(a => a.ExecDataRequest(c => c.Account.Type.Value));
            public Task<SymbolEntity> GetDefaultSymbol() => Actor.Call(a => a.ExecDataRequest(c => c.GetDefaultSymbol()));

            public Task<PluginFeedProvider> CreateFeedProvider()
            {
                return Actor.Call(a =>
                {
                    var historyHandler = new FeedHistoryProviderModel.Handler(a._feedHistory.Ref);
                    return new PluginFeedProvider(a._cache, a._rootDistributor, historyHandler, a.GetSyncContext());
                });
            }

            public Task<PluginTradeInfoProvider> CreateTradeProvider()
                => Actor.Call(a => new PluginTradeInfoProvider(a._cache, a.GetSyncContext()));

            public async Task<PluginTradeApiProvider.Handler> CreateTradeApi()
            {
                var apiRef = await Actor.Call(a => a._tradeApi.GetRef());
                return new PluginTradeApiProvider.Handler(apiRef);
            }

            public async Task<TradeHistoryProvider.Handler> CreateTradeHistory()
            {
                var apiRef = await Actor.Call(a => a._tradeHistory.GetRef());
                var provider = new TradeHistoryProvider.Handler(apiRef);
                await provider.Init();
                return provider;
            }
        }

        public class Data : Handler<ClientModel>, IQuoteDistributorSource, IMarketDataProvider
        {
            public Data(Ref<ClientModel> actorRef) : base(actorRef)
            {
                Cache = new EntityCache();
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

            public void StartCalculator()
            {
                Cache.Account.StartCalculator(this);
            }

            public IFeedSubscription SubscribeAll() => Distributor.SubscribeAll();

            public void ClearCache()
            {
                Cache.Clear();
            }

            public async Task Init()
            {
                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.OpenHandler();

                FeedHistory = new FeedHistoryProviderModel.Handler(await Actor.Call(a => a._feedHistory.Ref));
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

                await Actor.Call(a =>
                {
                    var subscription = a._rootDistributor.SubscribeAll();
                    a._feedSubcribers.Add(Ref, subscription);
                });
            }

            public async Task Deinit()
            {
                await Actor.Call(a => a.UnsyncListener(Ref));
                await Actor.Call(a => a._feedListeners.Remove(Ref));
                await Actor.Call(a => a._feedSubcribers.Remove(Ref));
                await Connection.CloseHandler();
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
                Actor.Send(a => a._feedSubcribers[Ref].Add(symbol, depth));
            }
        }

        private async Task Start()
        {
            logger.Debug("Start loading.");

            await _feedHistory.Start(_connection.FeedProxy, _connection.CurrentServer, _connection.CurrentLogin);

            logger.Debug("Feed history started.");

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

            PositionEntity[] positions = null;

            if (accInfo.Type == AccountTypes.Net)
            {
                positions = await tradeApi.GetPositions();
                logger.Debug("Loaded position snaphsot.");
            }

            foreach (var update in _cache.GetMergeUpdate(currencies))
                await ApplyUpdate(update);

            foreach (var update in _cache.GetMergeUpdate(symbols))
                await ApplyUpdate(update);


            var accUpdate = new AccountModel.Snapshot(accInfo, null, positions, accInfo.Assets);

            await ApplyUpdate(accUpdate);

            await initFeedTask;

            logger.Debug("Loaded quotes snaphsot.");

            var orderStream = Channel.NewInput<OrderEntity>();
            tradeApi.GetTradeRecords(CreateBlockingChannel(orderStream));

            while (await orderStream.ReadNext())
                await ApplyUpdate(new AccountModel.LoadOrderUpdate(orderStream.Current));

            logger.Debug("Loaded orders.");

            await FlushListeners();

            logger.Debug("Done loading.");

            // start multicasting

            MulticastUpdates();
            MulticastQuotes();
        }

        private async Task Stop()
        {
            try
            {
                logger.Debug("Stopping...");

                _updateQueue.Close();
                _feedQueue.Close(true);

                using (await _updateLock.GetLock("Stop")) { }

                logger.Debug("Stopped update stream.");

                using (await _feedLock.GetLock("Stop")) { }

                logger.Debug("Stopped quote stream.");

                await _feedHistory.Stop();

                logger.Debug("Stopped feed history.");

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

        private T ExecDataRequest<T>(Func<EntityCache, T> request)
        {
            CheckConnection();
            return request(_cache);
        }

        private void CheckConnection()
        {
            if (_connection.State != ConnectionModel.States.Online)
                throw new InvalidOperationException("No connection!");
        }

        #region Entity cache

        private EntityCacheUpdate AddListener(ActorRef handleRef, Channel<EntityCacheUpdate> channel)
        {
            _tradeListeners.Add(handleRef, channel);
            return _cache.GetAccSnapshot();
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
            using (await _updateLock.GetLock("loop"))
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
            try
            {
                if (update != null)
                {
                    update.Apply(_cache);

                    foreach (var listener in _tradeListeners.Values)
                        await listener.Write(update);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
            else if (item is PositionEntity)
                return _cache.Account.GetPositionUpdate((PositionEntity)item);
            else if (item is BalanceOperationReport)
                return _cache.Account.GetBalanceUpdate((BalanceOperationReport)item);

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

                var quotes = await _connection.FeedProxy.SubscribeToQuotes(groupSymbols, depth);
                logger.Debug("Subscribed to " + string.Join(",", groupSymbols));

                foreach (var q in quotes)
                    await ApplyQuote(q);
            }
        }

        private async void MulticastQuotes()
        {
            using (await _feedLock.GetLock("loop"))
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

        public async void ModifySubscription(string symbol, int depth)
        {
            //System.Diagnostics.Debug.WriteLine("ModifySubscription(" + symbol + ", " + depth + ")");

            try
            {
                await _connection.FeedProxy.SubscribeToQuotes(new string[] { symbol }, depth);
            }
            catch (Exception ex)
            {
                logger.Debug("Failed to modify quote subscription: " + ex.Message);
            }
        }

        #endregion
    }
}
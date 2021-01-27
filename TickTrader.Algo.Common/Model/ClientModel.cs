using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class ClientModel : Actor, IFeedSubscription
    {
        private IAlgoCoreLogger logger;

        private static int ActorNameIdSeed = 0;

        private ConnectionModel _connection;
        private FeedHistoryProviderModel.ControlHandler _feedHistory;
        private TradeHistoryProvider _tradeHistory;
        private PluginTradeApiProvider _tradeApi;
        private EntityCache _cache = new EntityCache();
        private Dictionary<ActorRef, Channel<EntityCacheUpdate>> _tradeListeners = new Dictionary<ActorRef, Channel<EntityCacheUpdate>>();
        private Dictionary<ActorRef, Channel<QuoteInfo>> _feedListeners = new Dictionary<ActorRef, Channel<QuoteInfo>>();
        private AsyncQueue<object> _updateQueue;
        private AsyncQueue<QuoteInfo> _feedQueue;
        private Ref<ClientModel> _ref;
        private QuoteDistributor _rootDistributor;
        private ActorSharp.Lib.AsyncLock _updateLock;
        private ActorSharp.Lib.AsyncLock _feedLock;
        private Dictionary<ActorRef, IFeedSubscription> _feedSubcribers = new Dictionary<ActorRef, IFeedSubscription>();
        private IFeedSubscription _defaultSubscription;

        protected override void ActorInit()
        {
            _ref = this.GetRef();
            _updateLock = new ActorSharp.Lib.AsyncLock();
            _feedLock = new ActorSharp.Lib.AsyncLock();
            _rootDistributor = new QuoteDistributor();
            _defaultSubscription = _rootDistributor.AddSubscription(q => { });
        }

        private void Init(ConnectionOptions connectionOptions, string historyFolder, FeedHistoryFolderOptions historyOptions, int loggerId)
        {
            logger = CoreLoggerFactory.GetLogger<ClientModel>(loggerId);

            _connection = new ConnectionModel(connectionOptions, loggerId);
            _feedHistory = new FeedHistoryProviderModel.ControlHandler(_connection, historyFolder, historyOptions, loggerId);
            _tradeHistory = new TradeHistoryProvider(_connection, loggerId);
            _tradeApi = new PluginTradeApiProvider(_connection);

            _tradeApi.OnExclusiveReport += er => _updateQueue.Enqueue(er);

            _connection.InitProxies += () =>
            {
                _updateQueue = new AsyncQueue<object>();
                _feedQueue = new AsyncQueue<QuoteInfo>();

                _connection.FeedProxy.Tick += FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                _connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            _connection.AsyncInitalizing += (s, c) => Start();

            _connection.AsyncDisconnected += (s, t) =>
            {
                _connection.FeedProxy.Tick -= FeedProxy_Tick;
                _connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                _connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;

                return Stop();
            };
        }

        private void FeedProxy_Tick(QuoteInfo q)
        {
            ContextSend(() => _feedQueue.Enqueue(q));
        }

        private void TradeProxy_ExecutionReport(ExecutionReport er)
        {
            ContextSend(() => _updateQueue.Enqueue(er));
        }

        private void TradeProxy_PositionReport(Domain.PositionExecReport pr)
        {
            ContextSend(() => _updateQueue.Enqueue(pr));
        }

        private void TradeProxy_BalanceOperation(Domain.BalanceOperation rep)
        {
            ContextSend(() => _updateQueue.Enqueue(rep));
        }

        public class ControlHandler : BlockingHandler<ClientModel>
        {
            public ControlHandler(ConnectionOptions options, string historyFolder, FeedHistoryFolderOptions hsitoryOptions, int loggerId)
                : base(SpawnLocal<ClientModel>(null, "ClientModel " + Interlocked.Increment(ref ActorNameIdSeed)))
            {
                ActorSend(a => a.Init(options, historyFolder, hsitoryOptions, loggerId));
            }

            public Data CreateDataHandler() => new Data(Actor);
        }

        public class ControlHandler2 : Handler<ClientModel>
        {
            public ControlHandler2(ConnectionOptions options, string historyFolder, FeedHistoryFolderOptions hsitoryOptions, int loggerId)
                : base(SpawnLocal<ClientModel>(null, "ClientModel " + Interlocked.Increment(ref ActorNameIdSeed)))
            {
                Actor.Send(a => a.Init(options, historyFolder, hsitoryOptions, loggerId));
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

            public Task<List<Domain.SymbolInfo>> GetSymbols() => Actor.Call(a => a.ExecDataRequest(c => c.GetSymbolsCopy()));
            public Task<List<CurrencyInfo>> GetCurrecnies() => Actor.Call(a => a.ExecDataRequest(c => c.GetCurrenciesCopy()));
            public Task<Domain.AccountInfo.Types.Type> GetAccountType() => Actor.Call(a => a.ExecDataRequest(c => c.Account.Type.Value));
            public Task<Domain.SymbolInfo> GetDefaultSymbol() => Actor.Call(a => a.ExecDataRequest(c => c.GetDefaultSymbol()));

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

        public class Data : Handler<ClientModel>, IFeedSubscription, IMarketDataProvider
        {
            private IFeedSubscription _defaultSubscription;

            public Data(Ref<ClientModel> actorRef) : base(actorRef)
            {
                Cache = new EntityCache();
                Distributor = new QuoteDistributor();
                _defaultSubscription = Distributor.AddSubscription(q => { });
            }

            public ConnectionModel.Handler Connection { get; private set; }
            public Ref<Data> Ref { get; private set; }
            public EntityCache Cache { get; private set; }
            public FeedHistoryProviderModel.Handler FeedHistory { get; private set; }
            public TradeHistoryProvider.Handler TradeHistory { get; private set; }
            public PluginTradeApiProvider.Handler TradeApi { get; private set; }
            public QuoteDistributor Distributor { get; }
            public IVarSet<string, SymbolInfo> Symbols => Cache.Symbols;
            public IVarSet<string, CurrencyInfo> Currencies => Cache.Currencies;

            protected override void ActorInit()
            {
                Ref = this.GetRef();
            }

            public void StartCalculator()
            {
                Cache.Account.StartCalculator(this);
            }

            public void ClearCache()
            {
                Cache.Clear();
            }

            public async Task Init()
            {
                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.OpenHandler();
                Connection.Connected += Connection_Connected;
                Connection.Disconnected += Connection_Disconnected;

                FeedHistory = new FeedHistoryProviderModel.Handler(await Actor.Call(a => a._feedHistory.Ref));
                await FeedHistory.Init();

                TradeHistory = new TradeHistoryProvider.Handler(await Actor.Call(a => a._tradeHistory.GetRef()));
                await TradeHistory.Init();

                TradeApi = new PluginTradeApiProvider.Handler(await Actor.Call(a => a._tradeApi.GetRef()));

                var updateStream = Channel.NewOutput<EntityCacheUpdate>(1000);
                var snapshot = await Actor.OpenChannel(updateStream, (a, c) => a.AddListener(Ref, c));
                ApplyUpdates(updateStream);

                var quoteStream = Channel.NewOutput<QuoteInfo>(1000);
                await Actor.OpenChannel(quoteStream, (a, c) => a._feedListeners.Add(Ref, c));
                ApplyQuotes(quoteStream);
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

            private async void ApplyQuotes(Channel<QuoteInfo> updateStream)
            {
                while (await updateStream.ReadNext())
                {
                    var quote = updateStream.Current;
                    Cache.ApplyQuote(quote);
                    Distributor.UpdateRate(quote);
                }
            }

            List<QuoteInfo> IFeedSubscription.Modify(List<FeedSubscriptionUpdate> updates)
            {
                Actor.Send(a => a.UpsertSubscription(Ref, updates));
                return null;
            }

            async Task<List<QuoteInfo>> IFeedSubscription.ModifyAsync(List<FeedSubscriptionUpdate> updates)
            {
                await Actor.Call(a => a.UpsertSubscription(Ref, updates));
                return null;
            }

            void IFeedSubscription.CancelAll()
            {
                Actor.Send(a => a.RemoveSubscription(Ref));
            }

            Task IFeedSubscription.CancelAllAsync()
            {
                return Actor.Call(a => a.RemoveSubscription(Ref));
            }

            private void Connection_Connected()
            {
                Distributor.Start(this);
                _defaultSubscription.AddOrModify(Cache.Symbols.Snapshot.Keys, 1);
            }

            private void Connection_Disconnected()
            {
                Distributor.Stop();
                _defaultSubscription.CancelAll();
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

            // symbols snapshot has to be applied before loading quotes snapshot
            foreach (var update in _cache.GetMergeUpdate(currencies))
                await ApplyUpdate(update);

            foreach (var update in _cache.GetMergeUpdate(symbols))
                await ApplyUpdate(update);

            var initFeedTask = LoadQuotesSnapshot(symbols.Select(s => s.Name));

            var accInfo = await getInfoTask;

            logger.Debug("Loaded account info.");

            Domain.PositionInfo[] positions = null;

            if (accInfo.Type == Domain.AccountInfo.Types.Type.Net)
            {
                positions = await tradeApi.GetPositions();
                logger.Debug("Loaded position snaphsot.");
            }

            var accUpdate = new AccountModel.Snapshot(accInfo, null, positions, accInfo.Assets);

            await ApplyUpdate(accUpdate);

            await initFeedTask;

            logger.Debug("Loaded quotes snaphsot.");

            var orderStream = Channel.NewInput<Domain.OrderInfo>();
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

                _defaultSubscription.CancelAll();
                _updateQueue.Close();
                _feedQueue.Close(true);

                using (await _updateLock.GetLock("Stop")) { }

                logger.Debug("Stopped update stream.");

                using (await _feedLock.GetLock("Stop")) { }

                logger.Debug("Stopped quote stream.");

                await _feedHistory.Stop();

                _rootDistributor.Stop(false);

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
            else if (item is Domain.PositionExecReport)
                return _cache.Account.GetPositionUpdate((Domain.PositionExecReport)item);
            else if (item is Domain.BalanceOperation)
                return _cache.Account.GetBalanceUpdate((Domain.BalanceOperation)item);

            return null;
        }

        #endregion

        #region Quote distribution

        private async Task LoadQuotesSnapshot(IEnumerable<string> allSymbols)
        {
            _rootDistributor.Start(this, allSymbols, false);

            // In order to have valid cache we need to subscribe to all symbols by depth 1
            // This will create subscription groups which handle quotes cache
            _defaultSubscription.AddOrModify(allSymbols, 1);

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

        private async Task ApplyQuote(QuoteInfo quote)
        {
            _cache.ApplyQuote(quote);
            _rootDistributor.UpdateRate(quote);

            foreach (var listener in _feedListeners.Values)
                await listener.Write(quote);
        }

        private void UpsertSubscription(ActorRef sender, List<FeedSubscriptionUpdate> updates)
        {
            var subscription = _feedSubcribers.GetOrAdd(sender, () => _rootDistributor.AddSubscription(q => { }));
            subscription.Modify(updates);
        }

        private void RemoveSubscription(ActorRef sender)
        {
            if (_feedSubcribers.TryGetValue(sender, out var sub))
            {
                sub.CancelAll();
                _feedSubcribers.Remove(sender);
            }
        }

        private async Task ModifySubscription(IEnumerable<string> symbols, int depth)
        {
            try
            {
                await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), depth);
            }
            catch (Exception ex)
            {
                logger.Debug($"Failed to modify quote subscription. Arguments Symbols = {string.Join(",", symbols)}, Depth = {depth}, Error = {ex}");
            }
        }

        List<QuoteInfo> IFeedSubscription.Modify(List<FeedSubscriptionUpdate> updates)
        {
            (this as IFeedSubscription).ModifyAsync(updates).Forget();

            return null;
        }

        async Task<List<QuoteInfo>> IFeedSubscription.ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            var removes = updates.Where(u => u.IsRemoveAction);
            var upserts = updates.Where(u => u.IsUpsertAction).GroupBy(u => u.Depth);

            foreach (var upsertGourp in upserts)
            {
                var depth = upsertGourp.Key;
                var symols = upsertGourp.Select(e => e.Symbol);
                await ModifySubscription(symols, depth);
            }

            return null;
        }

        void IFeedSubscription.CancelAll()
        {
        }

        Task IFeedSubscription.CancelAllAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}

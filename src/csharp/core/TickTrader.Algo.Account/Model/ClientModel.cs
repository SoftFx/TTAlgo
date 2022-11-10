using ActorSharp;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class ClientModel : ActorSharp.Actor
    {
        private static int _actorNameIdSeed = 0;

        private readonly ActorEventSource<EntityCacheUpdate> _tradeEventSrc = new ActorEventSource<EntityCacheUpdate>();
        private readonly ActorEventSource<QuoteInfo> _quoteEventSrc = new ActorEventSource<QuoteInfo>();

        private readonly EntityCache _cache = new EntityCache();

        private ChannelItemProcessor<object> _feedProcessor;
        private ChannelItemProcessor<object> _tradeProcessor;

        private FeedHistoryProviderModel.ControlHandler _feedHistory;
        private IActorRef _syncFeedProvider;

        private TradeHistoryProvider _tradeHistory;
        private PluginTradeApiProvider _tradeApi;

        private QuoteMonitoringModel _quoteMonitoring;
        private QuoteSubManager _rootQuoteSubManager;
        private BarSubManager _rootBarSubManager;
        private bool _allowSubModification;
        private ConnectionModel _connection;
        private TradeSubManager _tradeSubManager;

        private Ref<ClientModel> _ref;

        private IAlgoLogger _logger;


        protected override void ActorInit()
        {
            _ref = this.GetRef();
            var feedSubProvider = new FeedSubProviderWrapper(_ref);
            _rootQuoteSubManager = new QuoteSubManager(feedSubProvider);
            _tradeSubManager = new TradeSubManager(_cache.Account, new QuoteSubscription(_rootQuoteSubManager));
            _rootBarSubManager = new BarSubManager(feedSubProvider);
        }

        private void Init(AccountModelSettings settings)
        {
            var loggerId = settings.LoggerId;

            _logger = AlgoLoggerFactory.GetLogger<ClientModel>(loggerId);

            _connection = new ConnectionModel(settings.ConnectionSettings, loggerId);
            _tradeHistory = new TradeHistoryProvider(_connection, loggerId);
            _tradeApi = new PluginTradeApiProvider(_connection);
            _feedHistory = new FeedHistoryProviderModel.ControlHandler(/*settings.HistoryProviderSettings, */loggerId);
            _syncFeedProvider = SyncFeedProviderActor.Create(new FeedHistoryProviderModel.Handler(_feedHistory.Ref), new BarSubscription(_rootBarSubManager), loggerId);

            if (settings.Monitoring?.EnableQuoteMonitoring ?? false)
                _quoteMonitoring = new QuoteMonitoringModel(_connection, settings.Monitoring);

            _feedProcessor = ChannelItemProcessor<object>.CreateUnbounded($"{Name} feed loop", true);
            _tradeProcessor = ChannelItemProcessor<object>.CreateUnbounded($"{Name} trade loop", true);


            _tradeApi.OnExclusiveReport += er => _tradeProcessor.Add(er);

            _connection.InitProxies += () =>
            {
                _connection.FeedProxy.Tick += FeedProxy_Tick;
                _connection.FeedProxy.BarUpdate += FeedProxy_BarUpdate;
                _connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                _connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            _connection.AsyncInitalizing += (s, c) => Start();

            _connection.AsyncDisconnected += (s, t) =>
            {
                _connection.FeedProxy.Tick -= FeedProxy_Tick;
                _connection.FeedProxy.BarUpdate -= FeedProxy_BarUpdate;
                _connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                _connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                _connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;

                return Stop();
            };
        }

        private void FeedProxy_Tick(QuoteInfo q)
        {
            _quoteMonitoring?.CheckQuoteDelay(q);
            _feedProcessor.Add(q);
        }

        private void FeedProxy_BarUpdate(BarUpdate b)
        {
            _feedProcessor.Add(b);
        }

        private void TradeProxy_ExecutionReport(ExecutionReport er)
        {
            _tradeProcessor.Add(er);
        }

        private void TradeProxy_PositionReport(Domain.PositionExecReport pr)
        {
            _tradeProcessor.Add(pr);
        }

        private void TradeProxy_BalanceOperation(Domain.BalanceOperation rep)
        {
            _tradeProcessor.Add(rep);
        }

        private class FeedSubProviderWrapper : Handler<ClientModel>, IQuoteSubProvider, IBarSubProvider
        {
            public FeedSubProviderWrapper(Ref<ClientModel> actorRef)
                : base(actorRef)
            {
            }


            public void Modify(List<QuoteSubUpdate> updates) => Actor.Call(a => a.ModifyAsync(updates));

            public void Modify(List<BarSubUpdate> updates) => Actor.Call(a => a.ModifyAsync(updates));
        }

        public class ControlHandler : BlockingHandler<ClientModel>
        {
            private readonly string _loggerId;

            public ControlHandler(AccountModelSettings settings)
                : base(SpawnLocal<ClientModel>(null, $"ClientModel {Interlocked.Increment(ref _actorNameIdSeed)}"))
            {
                _loggerId = settings.LoggerId;
                ActorSend(a => a.Init(settings));
            }

            public Data CreateDataHandler() => new Data(Actor, _loggerId);
        }

        public class ControlHandler2 : Handler<ClientModel>
        {
            public ControlHandler2(AccountModelSettings settings)
                : base(SpawnLocal<ClientModel>(null, $"ClientModel {Interlocked.Increment(ref _actorNameIdSeed)}"))
            {
                Actor.Send(a => a.Init(settings));
            }

            public string Id => Actor.ActorName;

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
                    return new PluginFeedProvider(a._cache, a._rootQuoteSubManager, a._rootBarSubManager, historyHandler, a._syncFeedProvider);
                });
            }

            public Task<PluginTradeInfoProvider> CreateTradeProvider()
                => Actor.Call(a => new PluginTradeInfoProvider(a._cache));

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

        public class Data : Handler<ClientModel>, IMarketDataProvider
        {
            private readonly IAlgoLogger _logger;

            private ChannelConsumerWrapper<EntityCacheUpdate> _updateConsumer;
            private ChannelConsumerWrapper<QuoteInfo> _quoteConsumer;


            public Data(Ref<ClientModel> actorRef, string loggerId) : base(actorRef)
            {
                _logger = AlgoLoggerFactory.GetLogger($"{nameof(ClientModel)}.{nameof(Data)} {loggerId}");
                Cache = new EntityCache();
            }

            public string Id => Actor.ActorName;
            public ConnectionModel.Handler Connection { get; private set; }
            public Ref<Data> Ref { get; private set; }
            public EntityCache Cache { get; private set; }
            public FeedHistoryProviderModel.Handler FeedHistory { get; private set; }
            public TradeHistoryProvider.Handler TradeHistory { get; private set; }
            public PluginTradeApiProvider.Handler TradeApi { get; private set; }
            public QuoteDistributor Distributor { get; private set; }
            public IVarSet<string, SymbolInfo> Symbols => Cache.Symbols;
            public IVarSet<string, CurrencyInfo> Currencies => Cache.Currencies;
            public IQuoteSubManager QuoteSubManager { get; private set; }
            public BarDistributor BarDistributor { get; private set; }
            public IBarSubManager BarSubManager { get; private set; }
            public IActorRef SyncFeedProvider { get; private set; }

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
                QuoteSubManager = await Actor.Call(a => a._rootQuoteSubManager);
                BarSubManager = await Actor.Call(a => a._rootBarSubManager);

                Distributor = new QuoteDistributor(QuoteSubManager);
                BarDistributor = new BarDistributor(BarSubManager);

                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.OpenHandler();

                FeedHistory = new FeedHistoryProviderModel.Handler(await Actor.Call(a => a._feedHistory.Ref));
                await FeedHistory.Init();

                SyncFeedProvider = await Actor.Call(a => a._syncFeedProvider);

                TradeHistory = new TradeHistoryProvider.Handler(await Actor.Call(a => a._tradeHistory.GetRef()));
                await TradeHistory.Init();

                TradeApi = new PluginTradeApiProvider.Handler(await Actor.Call(a => a._tradeApi.GetRef()));

                var updateChannel = DefaultChannelFactory.CreateForOneToOne<EntityCacheUpdate>();
                _updateConsumer = new ChannelConsumerWrapper<EntityCacheUpdate>(updateChannel, "ApplyUpdates loop");
                await Actor.Call(a => a._tradeEventSrc.Subscribe(updateChannel));
                _updateConsumer.Start(ApplyUpdates);

                var quoteChannel = DefaultChannelFactory.CreateForOneToOne<QuoteInfo>();
                _quoteConsumer = new ChannelConsumerWrapper<QuoteInfo>(quoteChannel, "ApplyQuotes loop");
                await Actor.Call(a => a._quoteEventSrc.Subscribe(quoteChannel.Writer));
                _quoteConsumer.Start(ApplyQuotes);
            }

            public async Task Deinit()
            {
                await _updateConsumer.Stop();
                await _quoteConsumer.Stop();
                await Connection.CloseHandler();
            }

            private void ApplyUpdates(EntityCacheUpdate update)
            {
                try
                {
                    update.Apply(Cache);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to apply cache update");
                }
            }

            private void ApplyQuotes(QuoteInfo quote)
            {
                try
                {
                    Cache.ApplyQuote(quote);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to apply quote update");
                }
            }
        }

        private async Task Start()
        {
            _logger.Debug("Start loading.");

            await _feedHistory.Start(_connection.FeedProxy, _connection.CurrentServer, _connection.CurrentLogin);

            _logger.Debug("Feed history started.");

            var tradeApi = _connection.TradeProxy;
            var feedApi = _connection.FeedProxy;

            var getCurrenciesTask = feedApi.GetCurrencies();
            var getSymbolsTask = feedApi.GetSymbols();
            var getInfoTask = tradeApi.GetAccountInfo();

            var currencies = await getCurrenciesTask;

            _logger.Debug("Loaded currencies.");

            var symbols = await getSymbolsTask;

            _logger.Debug("Loaded symbols.");

            // symbols snapshot has to be applied before loading quotes snapshot
            foreach (var update in _cache.GetMergeUpdate(currencies))
                ApplyUpdate(update);

            foreach (var update in _cache.GetMergeUpdate(symbols))
                ApplyUpdate(update);

            var accInfo = await getInfoTask;

            _logger.Debug("Loaded account info.");

            Domain.PositionInfo[] positions = null;

            if (accInfo.Type == Domain.AccountInfo.Types.Type.Net)
            {
                positions = await tradeApi.GetPositions();
                _logger.Debug("Loaded position snaphsot.");
            }

            var accUpdate = new AccountModel.Snapshot(accInfo, null, positions, accInfo.Assets);

            ApplyUpdate(accUpdate);

            var orderStream = ActorChannel.NewInput<Domain.OrderInfo>();
            tradeApi.GetTradeRecords(CreateBlockingChannel(orderStream));

            while (await orderStream.ReadNext())
                ApplyUpdate(new AccountModel.LoadOrderUpdate(orderStream.Current));

            _tradeSubManager.Start();

            _logger.Debug("Loaded orders.");

            _allowSubModification = true;

            await Task.WhenAll(
                RestoreQuotesSubscription(symbols.Select(s => s.Name)),
                RestoreBarsSubscription());

            _logger.Debug("Restored feed subscriptions.");

            _logger.Debug("Done loading.");

            // start multicasting

            _tradeProcessor.Start(ApplyTradeUpdate);
            _feedProcessor.Start(ApplyFeedUpdate);
        }

        private async Task Stop()
        {
            _allowSubModification = false;

            try
            {
                _logger.Debug("Stopping...");

                await _tradeSubManager.Stop();
                _logger.Debug("Stopped trade subscription management.");

                await _tradeProcessor.Stop();
                _logger.Debug("Stopped trade stream.");

                await _feedProcessor.Stop();
                _logger.Debug("Stopped feed stream.");

                await SyncFeedProviderModel.Reset(_syncFeedProvider);

                await _feedHistory.Stop();
                _logger.Debug("Stopped feed history.");

                _logger.Debug("Done stopping.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
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

        private void ApplyUpdate(EntityCacheUpdate update)
        {
            try
            {
                if (update != null)
                {
                    update.Apply(_cache);

                    _tradeEventSrc.DispatchEvent(update);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void ApplyTradeUpdate(object update)
        {
            var cacheUpdate = CreateCacheUpdate(update);
            ApplyUpdate(cacheUpdate);
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

        private void ApplyFeedUpdate(object update)
        {
            if (update is QuoteInfo quote)
                ApplyQuote(quote);
            else if (update is BarUpdate bar)
                ApplyBar(bar);
            else
                _logger.Error($"Unexcepted feed update of type: {update.GetType().FullName}");
        }

        #region Quote distribution

        private async Task RestoreQuotesSubscription(IEnumerable<string> allSymbols)
        {
            var updates = _rootQuoteSubManager.InitUnwrap(allSymbols);

            await ModifyAsync(updates);
        }

        private void ApplyQuote(QuoteInfo quote)
        {
            _cache.ApplyQuote(quote);
            _rootQuoteSubManager.Dispatch(quote);

            _quoteEventSrc.DispatchEvent(quote);
        }

        private async Task<QuoteInfo[]> ModifySubscription(IEnumerable<string> symbols, int depth)
        {
            try
            {
                if (_allowSubModification)
                {
                    QuoteInfo[] snapshot = new QuoteInfo[0];
                    switch (depth)
                    {
                        case SubscriptionDepth.RemoveSub:
                            _logger.Error($"Removing subs not supported. Arguments Symbols = {string.Join(",", symbols)}, Depth = {depth}");
                            break;
                        case SubscriptionDepth.MaxDepth:
                            snapshot = await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), 0, null);
                            break;
                        case SubscriptionDepth.Ambient:
                            snapshot = await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), 1, 7);
                            break;
                        case SubscriptionDepth.Tick_S0:
                            snapshot = await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), 1, 0);
                            break;
                        case SubscriptionDepth.Tick_S1:
                            snapshot = await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), 1, 1);
                            break;
                        default:
                            snapshot = await _connection.FeedProxy.SubscribeToQuotes(symbols.ToArray(), depth, null);
                            break;
                    }

                    _logger.Debug("Subscribed to quotes with depth = " + depth + " to " + string.Join(",", symbols));
                    return snapshot;
                }
                else
                {
                    _logger.Debug($"Quote subscription modified while offline. Args Depth = {depth}, Symbols = {string.Join(",", symbols)}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to modify quote subscription. Arguments Symbols = {string.Join(",", symbols)}, Depth = {depth}, Error = {ex}");
            }
            return new QuoteInfo[0];
        }

        private async Task ModifyAsync(List<QuoteSubUpdate> updates)
        {
            var removes = updates.Where(u => u.IsRemoveAction);
            var upserts = updates.Where(u => u.IsUpsertAction).GroupBy(u => u.Depth);

            foreach (var upsertGroup in upserts)
            {
                var depth = upsertGroup.Key;
                var symbols = upsertGroup.Select(e => e.Symbol);

                var quotes = await ModifySubscription(symbols, depth);
                foreach (var q in quotes)
                    ApplyQuote(q);
            }

            if (removes.Any())
                await ModifySubscription(removes.Select(e => e.Symbol), SubscriptionDepth.Ambient);
        }

        #endregion

        #region Bar distribution

        private async Task RestoreBarsSubscription()
        {
            var updates = _rootBarSubManager.GetCurrentSubs();

            await ModifyAsync(updates);
        }

        private void ApplyBar(BarUpdate bar)
        {
            _rootBarSubManager.Dispatch(bar);
        }

        private async Task ModifyAsync(List<BarSubUpdate> updates)
        {
            if (updates.Count == 0)
                return;

            try
            {
                if (!_allowSubModification)
                {
                    _logger.Debug($"Bar subscription modified while offline. Args = {string.Join(", ", updates.Select(u => u.ToShortString()))}");
                }
                else
                {
                    SyncFeedProviderModel.NotifyBarSubChanged(_syncFeedProvider, updates);

                    await _connection.FeedProxy.SubscribeToBars(updates);

                    _logger.Debug($"Subscribed to bars with args = {string.Join(", ", updates.Select(u => u.ToShortString()))}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to modify bar subscription. Arguments updates = {string.Join(", ", updates.Select(u => u.ToShortString()))}, Error = {ex}");
            }
        }

        #endregion
    }
}

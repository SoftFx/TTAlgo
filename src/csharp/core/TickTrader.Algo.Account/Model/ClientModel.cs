using ActorSharp;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class ClientModel : Actor
    {
        private static int _actorNameIdSeed = 0;

        private readonly Dictionary<ActorRef, ActorChannel<EntityCacheUpdate>> _tradeListeners = new Dictionary<ActorRef, ActorChannel<EntityCacheUpdate>>();
        private readonly Dictionary<ActorRef, ActorChannel<QuoteInfo>> _feedListeners = new Dictionary<ActorRef, ActorChannel<QuoteInfo>>();

        private readonly EntityCache _cache = new EntityCache();

        private ChannelItemProcessor<QuoteInfo> _feedProcessor;
        private ChannelItemProcessor<object> _tradeProcessor;

        private FeedHistoryProviderModel.ControlHandler _feedHistory;

        private TradeHistoryProvider _tradeHistory;
        private PluginTradeApiProvider _tradeApi;

        private QuoteMonitoringModel _quoteMonitoring;
        private QuoteSubManager _rootSubManager;
        private bool _allowSubModification;
        private ConnectionModel _connection;
        private TradeSubManager _tradeSubManager;

        private Ref<ClientModel> _ref;

        private IAlgoLogger _logger;


        protected override void ActorInit()
        {
            _ref = this.GetRef();
            _rootSubManager = new QuoteSubManager(new QuoteSubProviderWrapper(_ref));
            _tradeSubManager = new TradeSubManager(_cache.Account, new QuoteSubscription(_rootSubManager));
        }

        private void Init(AccountModelSettings settings)
        {
            var loggerId = settings.LoggerId;

            _logger = AlgoLoggerFactory.GetLogger<ClientModel>(loggerId);

            _connection = new ConnectionModel(settings.ConnectionSettings, loggerId);
            _tradeHistory = new TradeHistoryProvider(_connection, loggerId);
            _tradeApi = new PluginTradeApiProvider(_connection);
            _feedHistory = new FeedHistoryProviderModel.ControlHandler(/*settings.HistoryProviderSettings, */loggerId);

            if (settings.Monitoring?.EnableQuoteMonitoring ?? false)
                _quoteMonitoring = new QuoteMonitoringModel(_connection, settings.Monitoring);

            _feedProcessor = ChannelItemProcessor<QuoteInfo>.CreateUnbounded($"{Name} feed loop", true);
            _tradeProcessor = ChannelItemProcessor<object>.CreateUnbounded($"{Name} trade loop", true);


            _tradeApi.OnExclusiveReport += er => _tradeProcessor.Add(er);

            _connection.InitProxies += () =>
            {
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
            _quoteMonitoring?.CheckQuoteDelay(q);
            _feedProcessor.Add(q);
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

        private class QuoteSubProviderWrapper : Handler<ClientModel>, IQuoteSubProvider
        {
            public QuoteSubProviderWrapper(Ref<ClientModel> actorRef)
                : base(actorRef)
            {
            }


            public void Modify(List<FeedSubscriptionUpdate> updates) => Actor.Call(a => a.ModifyAsync(updates));
        }

        public class ControlHandler : BlockingHandler<ClientModel>
        {
            public ControlHandler(AccountModelSettings settings)
                : base(SpawnLocal<ClientModel>(null, $"ClientModel {Interlocked.Increment(ref _actorNameIdSeed)}"))
            {
                ActorSend(a => a.Init(settings));
            }

            public Data CreateDataHandler() => new Data(Actor);
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
                    return new PluginFeedProvider(a._cache, a._rootSubManager, historyHandler, a.GetSyncContext());
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

        public class Data : Handler<ClientModel>, IMarketDataProvider
        {
            public Data(Ref<ClientModel> actorRef) : base(actorRef)
            {
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
                Distributor = new QuoteDistributor(await Actor.Call(a => a._rootSubManager));

                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.OpenHandler();

                FeedHistory = new FeedHistoryProviderModel.Handler(await Actor.Call(a => a._feedHistory.Ref));
                await FeedHistory.Init();

                TradeHistory = new TradeHistoryProvider.Handler(await Actor.Call(a => a._tradeHistory.GetRef()));
                await TradeHistory.Init();

                TradeApi = new PluginTradeApiProvider.Handler(await Actor.Call(a => a._tradeApi.GetRef()));

                var updateStream = ActorChannel.NewOutput<EntityCacheUpdate>(1000);
                var snapshot = await Actor.OpenChannel(updateStream, (a, c) => a.AddListener(Ref, c));
                ApplyUpdates(updateStream);

                var quoteStream = ActorChannel.NewOutput<QuoteInfo>(1000);
                await Actor.OpenChannel(quoteStream, (a, c) => a._feedListeners.Add(Ref, c));
                ApplyQuotes(quoteStream);
            }

            public async Task Deinit()
            {
                await Actor.Call(a => a.UnsyncListener(Ref));
                await Actor.Call(a => a._feedListeners.Remove(Ref));
                await Connection.CloseHandler();
            }

            private async void ApplyUpdates(ActorChannel<EntityCacheUpdate> updateStream)
            {
                while (await updateStream.ReadNext())
                    updateStream.Current.Apply(Cache);
            }

            private async void ApplyQuotes(ActorChannel<QuoteInfo> updateStream)
            {
                while (await updateStream.ReadNext())
                {
                    var quote = updateStream.Current;
                    Cache.ApplyQuote(quote);
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
                await ApplyUpdate(update);

            foreach (var update in _cache.GetMergeUpdate(symbols))
                await ApplyUpdate(update);

            var accInfo = await getInfoTask;

            _logger.Debug("Loaded account info.");

            Domain.PositionInfo[] positions = null;

            if (accInfo.Type == Domain.AccountInfo.Types.Type.Net)
            {
                positions = await tradeApi.GetPositions();
                _logger.Debug("Loaded position snaphsot.");
            }

            var accUpdate = new AccountModel.Snapshot(accInfo, null, positions, accInfo.Assets);

            await ApplyUpdate(accUpdate);

            var orderStream = ActorChannel.NewInput<Domain.OrderInfo>();
            tradeApi.GetTradeRecords(CreateBlockingChannel(orderStream));

            while (await orderStream.ReadNext())
                await ApplyUpdate(new AccountModel.LoadOrderUpdate(orderStream.Current));

            _tradeSubManager.Start();

            _logger.Debug("Loaded orders.");

            await LoadQuotesSnapshot(symbols.Select(s => s.Name));

            _logger.Debug("Loaded quotes snaphsot.");

            await FlushListeners();

            _logger.Debug("Done loading.");

            // start multicasting

            _tradeProcessor.Start(ApplyTradeUpdate);
            _feedProcessor.Start(ApplyQuote);
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

                await _feedHistory.Stop();
                _logger.Debug("Stopped feed history.");

                await FlushListeners();

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

        private EntityCacheUpdate AddListener(ActorRef handleRef, ActorChannel<EntityCacheUpdate> channel)
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
                _logger.Error(ex);
            }
        }

        private Task ApplyTradeUpdate(object update)
        {
            var cacheUpdate = CreateCacheUpdate(update);
            return ApplyUpdate(cacheUpdate);
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
            var updates = _rootSubManager.InitUnwrap(allSymbols);

            _allowSubModification = true;
            await ModifyAsync(updates);
        }

        private async Task ApplyQuote(QuoteInfo quote)
        {
            _cache.ApplyQuote(quote);
            _rootSubManager.Dispatch(quote);

            foreach (var listener in _feedListeners.Values)
                await listener.Write(quote);
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

                    _logger.Debug("Subscribed with depth = " + depth + " to " + string.Join(",", symbols));
                    return snapshot;
                }
                else
                {
                    _logger.Debug($"Subscription modified while offline. Args Depth = {depth}, Symbols = {string.Join(",", symbols)}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to modify quote subscription. Arguments Symbols = {string.Join(",", symbols)}, Depth = {depth}, Error = {ex}");
            }
            return new QuoteInfo[0];
        }

        private async Task ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            var removes = updates.Where(u => u.IsRemoveAction);
            var upserts = updates.Where(u => u.IsUpsertAction).GroupBy(u => u.Depth);

            foreach (var upsertGroup in upserts)
            {
                var depth = upsertGroup.Key;
                var symbols = upsertGroup.Select(e => e.Symbol);

                var quotes = await ModifySubscription(symbols, depth);
                foreach (var q in quotes)
                    await ApplyQuote(q);
            }

            if (removes.Any())
                await ModifySubscription(removes.Select(e => e.Symbol), SubscriptionDepth.Ambient);
        }

        #endregion
    }
}

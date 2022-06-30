using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Runtime
{
    internal class RuntimeInfoProvider : IPluginMetadata, IAccountInfoProvider, ITradeExecutor, ITradeHistoryProvider, IFeedProvider, IFeedHistoryProvider
    {
        private class RuntimeContext : Actor { }

        private readonly Ref<RuntimeContext> _context;
        private readonly ISyncContext _sync;
        private readonly RemoteAccountProxy _account;
        private readonly IDisposable _orderUpdateSub, _positionUpdateSub, _balanceUpdateSub, _rateUpdateSub, _rateListUpdateSub;

        private List<CurrencyInfo> _currencies;
        private List<SymbolInfo> _symbols;
        private List<FullQuoteInfo> _quotes;
        private AccountInfo _accInfo;
        private List<OrderInfo> _orders;
        private List<PositionInfo> _positions;


        public RuntimeInfoProvider(RemoteAccountProxy account)
        {
            _account = account;

            _context = Actor.SpawnLocal<RuntimeContext>(null, $"Runtime {Guid.NewGuid()}");
            _sync = _context.GetSyncContext();

            _orderUpdateSub = _account.OrderUpdated.Subscribe(o => OrderUpdated?.Invoke(o));
            _positionUpdateSub = _account.PositionUpdated.Subscribe(p => PositionUpdated?.Invoke(p));
            _balanceUpdateSub = _account.BalanceUpdated.Subscribe(b => BalanceUpdated?.Invoke(b));

            _rateUpdateSub = _account.RateUpdated.Subscribe(q => RateUpdated?.Invoke(q));
            _rateListUpdateSub = _account.RatesUpdated.Subscribe(q => RatesUpdated?.Invoke(q));
        }


        public async Task PreLoad()
        {
            _currencies = await RunOnThreadPool(_account.GetCurrencyListAsync);
            _symbols = await RunOnThreadPool(_account.GetSymbolListAsync);
            _quotes = await RunOnThreadPool(_account.GetLastQuotesListAsync);

            _accInfo = await RunOnThreadPool(_account.GetAccountInfoAsync);
            _orders = await RunOnThreadPool(_account.GetOrderListAsync);
            _positions = await RunOnThreadPool(_account.GetPositionListAsync);

            ApplyLastQuoteListToSymbols();
        }

        public void Dispose()
        {
            _orderUpdateSub.Dispose();
            _positionUpdateSub.Dispose();
            _balanceUpdateSub.Dispose();
            _rateUpdateSub.Dispose();
            _rateListUpdateSub.Dispose();
        }


        private Task RunOnThreadPool(Func<Task> func)
        {
            return Task.Run(() => func());
        }

        private Task<T> RunOnThreadPool<T>(Func<Task<T>> func)
        {
            return Task.Run(() => func());
        }

        private void ApplyLastQuoteListToSymbols()
        {
            var symbolDict = _symbols.ToDictionary(k => k.Name, v => v);

            foreach (var quote in _quotes)
                symbolDict[quote.Symbol].UpdateRate(QuoteInfo.Create(quote));
        }

        #region IPluginMetadata

        public IEnumerable<CurrencyInfo> GetCurrencyMetadata()
        {
            return _currencies;
        }

        public IEnumerable<SymbolInfo> GetSymbolMetadata()
        {
            return _symbols;
        }

        public IEnumerable<FullQuoteInfo> GetLastQuoteMetadata()
        {
            return _quotes;
        }

        #endregion IPluginMetadata

        #region IAccountInfoProvider

        public event Action<OrderExecReport> OrderUpdated;
        public event Action<PositionExecReport> PositionUpdated;
        public event Action<BalanceOperation> BalanceUpdated;

        public AccountInfo GetAccountInfo()
        {
            return _accInfo;
        }

        public List<OrderInfo> GetOrders()
        {
            return _orders;
        }

        public List<PositionInfo> GetPositions()
        {
            return _positions;
        }

        public void SyncInvoke(Action action)
        {
            _sync.Invoke(action);
        }

        #endregion IAccountInfoProvider

        #region ITradeExecutor

        public void SendOpenOrder(OpenOrderRequest request)
        {
            _account.SendOpenOrder(request);
        }

        public void SendModifyOrder(ModifyOrderRequest request)
        {
            _account.SendModifyOrder(request);
        }

        public void SendCloseOrder(CloseOrderRequest request)
        {
            _account.SendCloseOrder(request);
        }

        public void SendCancelOrder(CancelOrderRequest request)
        {
            _account.SendCancelOrder(request);
        }

        #endregion ITradeExecutor

        #region ITradeHistoryProvider

        public IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, HistoryRequestOptions options)
        {
            return _account.GetTradeHistory(new TradeHistoryRequest { From = from?.ToUniversalTime().ToTimestamp(), To = to?.ToUniversalTime().ToTimestamp(), Options = options });
        }

        public IAsyncPagedEnumerator<TriggerReportInfo> GetTriggerHistory(DateTime? from, DateTime? to, HistoryRequestOptions options)
        {
            return _account.GetTriggerHistory(new TriggerHistoryRequest { From = from?.ToUniversalTime().ToTimestamp(), To = to?.ToUniversalTime().ToTimestamp(), Options = options });
        }

        #endregion ITradeHistoryProvider

        #region IFeedProvider

        public event Action<QuoteInfo> RateUpdated;
        public event Action<List<QuoteInfo>> RatesUpdated;

        public ISyncContext Sync => _sync;

        public List<QuoteInfo> GetSnapshot()
        {
            return GetSnapshotAsync().GetAwaiter().GetResult();
        }

        public Task<List<QuoteInfo>> GetSnapshotAsync()
        {
            return RunOnThreadPool(_account.GetFeedSnapshotAsync);
        }

        public IQuoteSub GetSubscription()
        {
            return _account.GetSubscription();
        }

        #endregion IFeedProvider

        #region IFeedHistoryProvider

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return QueryBarsAsync(symbol, marketSide, timeframe, from, to).GetAwaiter().GetResult();
        }

        public List<BarData> QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            return QueryBarsAsync(symbol, marketSide, timeframe, from, count).GetAwaiter().GetResult();
        }

        public List<QuoteInfo> QueryQuotes(string symbol, Timestamp from, Timestamp to, bool level2)
        {
            return QueryQuotesAsync(symbol, from, to, level2).GetAwaiter().GetResult();
        }

        public List<QuoteInfo> QueryQuotes(string symbol, Timestamp from, int count, bool level2)
        {
            return QueryQuotesAsync(symbol, from, count, level2).GetAwaiter().GetResult();
        }

        public Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            var request = new BarListRequest
            {
                Symbol = symbol,
                MarketSide = marketSide,
                Timeframe = timeframe,
                From = from,
                To = to,
            };
            return RunOnThreadPool(() => _account.GetBarListAsync(request));
        }

        public Task<List<BarData>> QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            var request = new BarListRequest
            {
                Symbol = symbol,
                MarketSide = marketSide,
                Timeframe = timeframe,
                From = from,
                Count = count,
            };
            return RunOnThreadPool(() => _account.GetBarListAsync(request));
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, Timestamp from, Timestamp to, bool level2)
        {
            var request = new QuoteListRequest
            {
                Symbol = symbol,
                From = from,
                To = to,
                Level2 = level2,
            };
            return RunOnThreadPool(() => _account.GetQuoteListAsync(request));
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbol, Timestamp from, int count, bool level2)
        {
            var request = new QuoteListRequest
            {
                Symbol = symbol,
                From = from,
                Count = count,
                Level2 = level2,
            };
            return RunOnThreadPool(() => _account.GetQuoteListAsync(request));
        }

        #endregion IFeedHistoryProvider
    }
}

using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class RuntimeInfoProvider : IPluginMetadata, IAccountInfoProvider, ITradeExecutor, ITradeHistoryProvider, IFeedProvider, IFeedHistoryProvider
    {
        private class RuntimeContext : Actor { }

        private readonly Ref<RuntimeContext> _context;
        private readonly ISyncContext _sync;

        private readonly RemoteAccountProxy _account;

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

            _account.OrderUpdated += o => OrderUpdated?.Invoke(o);
            _account.PositionUpdated += p => PositionUpdated?.Invoke(p);
            _account.BalanceUpdated += b => BalanceUpdated?.Invoke(b);

            _account.RateUpdated += q => RateUpdated?.Invoke(q);
            _account.RatesUpdated += q => RatesUpdated?.Invoke(q);
        }


        public async Task PreLoad()
        {
            _currencies = await _account.GetCurrencyListAsync();
            _symbols = await _account.GetSymbolListAsync();
            _quotes = await _account.GetLastQuotesListAsync();

            _accInfo = await _account.GetAccountInfoAsync();
            _orders = await _account.GetOrderListAsync();
            _positions = await _account.GetPositionListAsync();

            ApplyLastQuoteListToSymbols();
        }

        private void ApplyLastQuoteListToSymbols()
        {
            var symbolDict = _symbols.ToDictionary(k => k.Name, v => v);

            foreach (var quote in _quotes)
                symbolDict[quote.Symbol].UpdateRate(new QuoteInfo(quote));
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

        public event Action<Domain.OrderExecReport> OrderUpdated;
        public event Action<Domain.PositionExecReport> PositionUpdated;
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

        public IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, TradeHistoryRequestOptions options)
        {
            return _account.GetTradeHistory(new TradeHistoryRequest { From = from?.ToUniversalTime().ToTimestamp(), To = to?.ToUniversalTime().ToTimestamp(), Options = options });
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
            return _account.GetFeedSnapshotAsync();
        }

        public IFeedSubscription GetSubscription()
        {
            return _account.GetSubscription();
        }

        public List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates)
        {
            var request = new ModifyFeedSubscriptionRequest();
            request.Updates.AddRange(updates);
            return _account.ModifyFeedSubscription(request);
            //return ModifyAsync(updates).GetAwaiter().GetResult();
        }

        public Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            var request = new ModifyFeedSubscriptionRequest();
            request.Updates.AddRange(updates);
            return _account.ModifyFeedSubscriptionAsync(request);
        }

        public void CancelAll()
        {
            CancelAllAsync().GetAwaiter().GetResult();
        }

        public Task CancelAllAsync()
        {
            return _account.CancelAllFeedSubscriptionsAsync();
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
            return _account.GetBarListAsync(request);
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
            return _account.GetBarListAsync(request);
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
            return _account.GetQuoteListAsync(request);
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
            return _account.GetQuoteListAsync(request);
        }

        #endregion IFeedHistoryProvider
    }
}

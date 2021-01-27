﻿using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class RuntimeInfoProvider : CrossDomainObject, IPluginMetadata, IAccountInfoProvider, ITradeExecutor, ITradeHistoryProvider, IFeedProvider, IFeedHistoryProvider
    {
        private class RuntimeContext : Actor { }

        private readonly Ref<RuntimeContext> _context;
        private readonly ISyncContext _sync;

        private readonly UnitRuntimeV1Handler _handler;

        private List<CurrencyInfo> _currencies;
        private List<SymbolInfo> _symbols;
        private AccountInfo _accInfo;
        private List<OrderInfo> _orders;
        private List<PositionInfo> _positions;


        public RuntimeInfoProvider(UnitRuntimeV1Handler handler)
        {
            _handler = handler;

            _context = Actor.SpawnLocal<RuntimeContext>(null, $"Runtime {Guid.NewGuid()}");
            _sync = _context.GetSyncContext();

            _handler.OrderUpdated += o => OrderUpdated?.Invoke(o);
            _handler.PositionUpdated += p => PositionUpdated?.Invoke(p);
            _handler.BalanceUpdated += b => BalanceUpdated?.Invoke(b);

            _handler.RateUpdated += q => RateUpdated?.Invoke(q);
            _handler.RatesUpdated += q => RatesUpdated?.Invoke(q);
        }


        public async Task PreLoad()
        {
            _currencies = await _handler.GetCurrencyListAsync();
            _symbols = await _handler.GetSymbolListAsync();

            _accInfo = await _handler.GetAccountInfoAsync();
            _orders = await _handler.GetOrderListAsync();
            _positions = await _handler.GetPositionListAsync();
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
            _handler.SendOpenOrder(request);
        }

        public void SendModifyOrder(ModifyOrderRequest request)
        {
            _handler.SendModifyOrder(request);
        }

        public void SendCloseOrder(CloseOrderRequest request)
        {
            _handler.SendCloseOrder(request);
        }

        public void SendCancelOrder(CancelOrderRequest request)
        {
            _handler.SendCancelOrder(request);
        }

        #endregion ITradeExecutor

        #region ITradeHistoryProvider

        public IAsyncPagedEnumerator<TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, TradeHistoryRequestOptions options)
        {
            return _handler.GetTradeHistory(new TradeHistoryRequest { From = from?.ToUniversalTime().ToTimestamp(), To = to?.ToUniversalTime().ToTimestamp(), Options = options });
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
            return _handler.GetFeedSnapshotAsync();
        }

        public List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates)
        {
            var request = new ModifyFeedSubscriptionRequest();
            request.Updates.AddRange(updates);
            return _handler.ModifyFeedSubscription(request);
            //return ModifyAsync(updates).GetAwaiter().GetResult();
        }

        public Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            var request = new ModifyFeedSubscriptionRequest();
            request.Updates.AddRange(updates);
            return _handler.ModifyFeedSubscriptionAsync(request);
        }

        public void CancelAll()
        {
            CancelAllAsync().GetAwaiter().GetResult();
        }

        public Task CancelAllAsync()
        {
            return _handler.CancelAllFeedSubscriptionsAsync();
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
            return _handler.GetBarListAsync(request);
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
            return _handler.GetBarListAsync(request);
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
            return _handler.GetQuoteListAsync(request);
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
            return _handler.GetQuoteListAsync(request);
        }

        #endregion IFeedHistoryProvider
    }
}

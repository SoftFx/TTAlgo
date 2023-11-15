using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class AccountRpcHandler
    {
        private readonly IAccountProxy _account;
        private readonly RpcSession _session;
        private readonly string _accId;
        private readonly IAlgoLogger _logger;
        private readonly ConcurrentDictionary<string, object> _pendingRequestHandlers = new();
        private readonly QuoteSubTracker _quoteSubTracker = new();
        private readonly Dictionary<string, FullQuoteInfo> _delayedQuotes = new();

        private IQuoteSub _quoteSub;
        private IBarSub _barSub;
        private bool _disposed;


        public string SessionId => _session.Id;

        public RpcSessionState SessionState => _session.State;


        public AccountRpcHandler(IAccountProxy account, RpcSession session, string accId)
        {
            _account = account;
            _session = session;
            _accId = accId;
            _logger = AlgoLoggerFactory.GetLogger<AccountRpcHandler>(accId);

            _quoteSub = account.Feed.GetQuoteSub();
            _barSub = account.Feed.GetBarSub();

            _ = DispatchDelayedQuotesLoop();
        }


        public void Dispose()
        {
            _disposed = true;
            _quoteSub.Dispose();
            _quoteSub = null;
            _barSub.Dispose();
            _barSub = null;
        }


        public Task<Any> HandleRequest(string callId, Any payload)
        {
            if (payload.Is(ConnectionInfoRequest.Descriptor))
                return ConnectionInfoRequestHandler();
            else if (payload.Is(CurrencyListRequest.Descriptor))
                return CurrencyListRequestHandler();
            else if (payload.Is(SymbolListRequest.Descriptor))
                return SymbolListRequestHandler();
            else if (payload.Is(LastQuoteListRequest.Descriptor))
                return LastQuoteListRequestHandler();
            else if (payload.Is(AccountInfoRequest.Descriptor))
                return AccountInfoRequestHandler();
            else if (payload.Is(OrderListRequest.Descriptor))
                return OrderListRequestHandler(callId);
            else if (payload.Is(PositionListRequest.Descriptor))
                return PositionListRequestHandler(callId);
            else if (payload.Is(OpenOrderRequest.Descriptor))
                return OpenOrderRequestHandler(payload);
            else if (payload.Is(ModifyOrderRequest.Descriptor))
                return ModifyOrderRequestHandler(payload);
            else if (payload.Is(CloseOrderRequest.Descriptor))
                return CloseOrderRequestHandler(payload);
            else if (payload.Is(CancelOrderRequest.Descriptor))
                return CancelOrderRequestHandler(payload);
            else if (payload.Is(TradeHistoryRequest.Descriptor))
                return TradeHistoryRequestHandler(callId, payload);
            else if (payload.Is(TradeHistoryRequestNextPage.Descriptor))
                return TradeHistoryRequestNextPageHandler(callId);
            else if (payload.Is(TradeHistoryRequestDispose.Descriptor))
                return TradeHistoryRequestDisposeHandler(callId);
            else if (payload.Is(TriggerHistoryRequest.Descriptor))
                return TriggerHistoryRequestHandler(callId, payload);
            else if (payload.Is(TriggerHistoryRequestNextPage.Descriptor))
                return TriggerHistoryRequestNextPageHandler(callId);
            else if (payload.Is(TriggerHistoryRequestDispose.Descriptor))
                return TriggerHistoryRequestDisposeHandler(callId);
            else if (payload.Is(QuoteSnapshotRequest.Descriptor))
                return QuoteSnapshotRequestHandler();
            else if (payload.Is(ModifyQuoteSubRequest.Descriptor))
                return ModifyQuoteSubRequestHandler(payload);
            else if (payload.Is(ModifyBarSubRequest.Descriptor))
                return ModifyBarSubRequestHandler(payload);
            else if (payload.Is(BarListRequest.Descriptor))
                return BarListRequestHandler(payload);
            else if (payload.Is(QuoteListRequest.Descriptor))
                return QuoteListRequestHandler(payload);

            return Task.FromResult(default(Any));
        }

        public void DispatchNotification(AccountRpcNotification msg)
        {
            if (msg.Payload is FullQuoteInfo quote)
            {
                if (ThrottleQuote(quote))
                    return;
            }

            _session.Tell(msg.Message);
        }


        private Task<Any> ConnectionInfoRequestHandler()
        {
            return Task.FromResult(Any.Pack(new ConnectionInfoResponse { ConnectionInfo = _account.Id }));
        }

        private Task<Any> CurrencyListRequestHandler()
        {
            var response = new CurrencyListResponse();
            response.Currencies.Add(_account.Metadata.GetCurrencyMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> SymbolListRequestHandler()
        {
            var response = new SymbolListResponse();
            response.Symbols.Add(_account.Metadata.GetSymbolMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> LastQuoteListRequestHandler()
        {
            var response = new LastQuoteListResponse();
            response.Quotes.Add(_account.Metadata.GetLastQuoteMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> AccountInfoRequestHandler()
        {
            var response = new AccountInfoResponse();
            response.Account = _account.AccInfoProvider.GetAccountInfo();
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> OrderListRequestHandler(string callId)
        {
            const int chunkSize = 10;

            var response = new OrderListResponse { IsFinal = false };
            var orders = _account.AccInfoProvider.GetOrders();
            var cnt = orders.Count;

            var nextFlush = chunkSize;
            for (var i = 0; i < cnt; i++)
            {
                if (i == nextFlush)
                {
                    nextFlush += chunkSize;
                    _session.Tell(RpcMessage.Response(callId, Any.Pack(response)));
                    response.Orders.Clear();
                }
                response.Orders.Add(orders[i]);
            }
            response.IsFinal = true;
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> PositionListRequestHandler(string callId)
        {
            const int chunkSize = 10;

            var response = new PositionListResponse { IsFinal = false };
            var positions = _account.AccInfoProvider.GetPositions();
            var cnt = positions.Count;

            var nextFlush = chunkSize;
            for (var i = 0; i < cnt; i++)
            {
                if (i == nextFlush)
                {
                    nextFlush += chunkSize;
                    _session.Tell(RpcMessage.Response(callId, Any.Pack(response)));
                    response.Positions.Clear();
                }
                response.Positions.Add(positions[i]);
            }
            response.IsFinal = true;
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> OpenOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<OpenOrderRequest>();
            _account.TradeExecutor.SendOpenOrder(request);
            return Task.FromResult(RpcHandler.VoidResponse);
        }

        private Task<Any> ModifyOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyOrderRequest>();
            _account.TradeExecutor.SendModifyOrder(request);
            return Task.FromResult(RpcHandler.VoidResponse);
        }

        private Task<Any> CloseOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CloseOrderRequest>();
            _account.TradeExecutor.SendCloseOrder(request);
            return Task.FromResult(RpcHandler.VoidResponse);
        }

        private Task<Any> CancelOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CancelOrderRequest>();
            _account.TradeExecutor.SendCancelOrder(request);
            return Task.FromResult(RpcHandler.VoidResponse);
        }

        private Task<Any> TradeHistoryRequestHandler(string callId, Any payload)
        {
            var request = payload.Unpack<TradeHistoryRequest>();
            var enumerator = _account.TradeHistoryProvider.GetTradeHistory(request.From, request.To, request.Options);
            if (enumerator != null)
            {
                _pendingRequestHandlers.TryAdd(callId, enumerator);
            }
            return Task.FromResult<Any>(null);
        }

        private async Task<Any> TradeHistoryRequestNextPageHandler(string callId)
        {
            _pendingRequestHandlers.TryGetValue(callId, out var state);
            var enumerator = (IAsyncPagedEnumerator<TradeReportInfo>)state;
            var page = await enumerator.GetNextPage();
            var response = new TradeHistoryPageResponse();
            if (page == null || page.Count == 0)
            {
                _pendingRequestHandlers.TryRemove(callId, out _);
                enumerator.Dispose();
            }
            else
            {
                response.Reports.AddRange(page);
            }
            return Any.Pack(response);
        }

        private Task<Any> TradeHistoryRequestDisposeHandler(string callId)
        {
            if (_pendingRequestHandlers.TryRemove(callId, out var state))
            {
                var enumerator = (IAsyncPagedEnumerator<TradeReportInfo>)state;
                enumerator.Dispose();
            }
            return Task.FromResult<Any>(null);
        }

        private Task<Any> TriggerHistoryRequestHandler(string callId, Any payload)
        {
            var request = payload.Unpack<TriggerHistoryRequest>();
            var enumerator = _account.TradeHistoryProvider.GetTriggerHistory(request.From, request.To, request.Options);
            if (enumerator != null)
            {
                _pendingRequestHandlers.TryAdd(callId, enumerator);
            }
            return Task.FromResult<Any>(null);
        }

        private async Task<Any> TriggerHistoryRequestNextPageHandler(string callId)
        {
            _pendingRequestHandlers.TryGetValue(callId, out var state);
            var enumerator = (IAsyncPagedEnumerator<TriggerReportInfo>)state;
            var page = await enumerator.GetNextPage();
            var response = new TriggerHistoryPageResponse();
            if (page == null || page.Count == 0)
            {
                _pendingRequestHandlers.TryRemove(callId, out _);
                enumerator.Dispose();
            }
            else
            {
                response.Reports.AddRange(page);
            }
            return Any.Pack(response);
        }

        private Task<Any> TriggerHistoryRequestDisposeHandler(string callId)
        {
            if (_pendingRequestHandlers.TryRemove(callId, out var state))
            {
                var enumerator = (IAsyncPagedEnumerator<TriggerReportInfo>)state;
                enumerator.Dispose();
            }
            return Task.FromResult<Any>(null);
        }

        private Task<Any> QuoteSnapshotRequestHandler()
        {
            var response = new QuotePage();
            response.Quotes.AddRange(_account.Feed.GetQuoteSnapshot().Select(q => q.GetFullQuote()));
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> ModifyQuoteSubRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyQuoteSubRequest>();
            var response = new QuotePage();
            if (request.Updates.Count == 1)
            {
                _quoteSub.Modify(request.Updates[0]);
                _quoteSubTracker.ApplyUpdate(request.Updates[0]);
            }
            else
            {
                _quoteSub.Modify(request.Updates.ToList());
                _quoteSubTracker.ApplyUpdates(request.Updates);
            }
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> ModifyBarSubRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyBarSubRequest>();
            var response = new BarPage();
            if (request.Updates.Count == 1)
                _barSub.Modify(request.Updates[0]);
            else _barSub.Modify(request.Updates.ToList());
            return Task.FromResult(Any.Pack(response));
        }

        private async Task<Any> BarListRequestHandler(Any payload)
        {
            var request = payload.Unpack<BarListRequest>();
            var symbol = request.Symbol;
            var marketSide = request.MarketSide;
            var timeframe = request.Timeframe;
            var count = request.Count;
            var response = new BarChunk
            {
                Symbol = symbol,
                MarketSide = marketSide,
                Timeframe = timeframe,
            };
            var barList = await (count.HasValue
                ? _account.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, count.Value)
                : _account.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, request.To));
            response.Bars.AddRange(barList);

            return Any.Pack(response);
        }

        private async Task<Any> QuoteListRequestHandler(Any payload)
        {
            var request = payload.Unpack<QuoteListRequest>();
            var symbol = request.Symbol;
            var count = request.Count;
            var response = new QuoteChunk { Symbol = symbol, };
            var quoteList = await (count.HasValue
                ? _account.FeedHistory.QueryQuotesAsync(symbol, request.From, count.Value, request.Level2)
                : _account.FeedHistory.QueryQuotesAsync(symbol, request.From, request.To, request.Level2));
            response.Quotes.AddRange(quoteList.Select(q => q.GetData()));

            return Any.Pack(response);
        }

        private bool ThrottleQuote(FullQuoteInfo quote)
        {
            const bool EnableQuoteThrottling = true;
            if (EnableQuoteThrottling && !_quoteSubTracker.HasSymbolSubs(quote.Symbol))
            {
                lock (_delayedQuotes)
                {
                    _delayedQuotes[quote.Symbol] = quote;
                }
                return true;
            }
            return false;
        }

        private async Task DispatchDelayedQuotesLoop()
        {
            var quotePage = new QuotePage();
            var quotesList = quotePage.Quotes; // keep buffer preallocated to share between flushes
            while (!_disposed)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    quotesList.Clear(); // just in case of exception on last call
                    lock (_delayedQuotes)
                    {
                        quotesList.AddRange(_delayedQuotes.Values);
                        _delayedQuotes.Clear();
                    }

                    _session.Tell(RpcMessage.Notification(_accId, quotePage));

                    quotesList.Clear();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to delayed quote page to session {SessionId}");
                }
            }
        }
    }
}

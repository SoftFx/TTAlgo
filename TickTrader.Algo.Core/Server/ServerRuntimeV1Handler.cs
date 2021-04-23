using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class ServerRuntimeV1Handler : IRpcHandler
    {
        private static readonly Any VoidResponse = Any.Pack(new VoidResponse());

        private readonly AlgoServer _server;
        private RuntimeModel _runtime;
        private RpcSession _session;

        private ConcurrentDictionary<string, object> _pendingRequestHandlers;


        public ServerRuntimeV1Handler(AlgoServer server)
        {
            _server = server;

            _pendingRequestHandlers = new ConcurrentDictionary<string, object>();
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
        }

        public void HandleNotification(string proxyId, string callId, Any payload)
        {
            if (payload.Is(PluginLogRecord.Descriptor))
                PluginLogRecordHandler(proxyId, payload);
            else if (payload.Is(PluginError.Descriptor))
                PluginErrorHandler(proxyId, payload);
            else if (payload.Is(PluginStopped.Descriptor))
                PluginStoppedHandler(proxyId);
            else if (payload.Is(DataSeriesUpdate.Descriptor))
                DataSeriesUpdateHandler(proxyId, payload);

        }

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(AttachRuntimeRequest.Descriptor))
            {
                var request = payload.Unpack<AttachRuntimeRequest>();
                if (_runtime != null)
                {
                    return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Runtime already attached!" }));
                }
                if (_server.TryGetRuntime(request.Id, out var runtime))
                {
                    _runtime = runtime;

                    _runtime.OnAttached(_session);

                    return Task.FromResult(Any.Pack(new AttachRuntimeResponse { Success = true }));
                }
                else
                {
                    return Task.FromResult(Any.Pack(new AttachRuntimeResponse { Success = false }));
                }
            }
            else if (payload.Is(RuntimeConfigRequest.Descriptor))
                return RuntimeConfigRequestHandler();
            else if (payload.Is(PackagePathRequest.Descriptor))
                return PackagePathRequestHandler(payload);
            else if (payload.Is(ExecutorConfigRequest.Descriptor))
                return ExecutorConfigRequestHandler(proxyId);
            else if (payload.Is(AttachAccountRequest.Descriptor))
                return AttachAccountRequestHandler(payload);
            else if (payload.Is(DetachAccountRequest.Descriptor))
                return DetachAccountRequestHandler(payload);
            else if (payload.Is(ConnectionInfoRequest.Descriptor))
                return ConnectionInfoRequestHandler(proxyId);
            else if (payload.Is(CurrencyListRequest.Descriptor))
                return CurrencyListRequestHandler(proxyId);
            else if (payload.Is(SymbolListRequest.Descriptor))
                return SymbolListRequestHandler(proxyId);
            else if (payload.Is(LastQuoteListRequest.Descriptor))
                return LastQuoteListRequestHandler(proxyId);
            else if (payload.Is(AccountInfoRequest.Descriptor))
                return AccountInfoRequestHandler(proxyId);
            else if (payload.Is(OrderListRequest.Descriptor))
                return OrderListRequestHandler(proxyId, callId);
            else if (payload.Is(PositionListRequest.Descriptor))
                return PositionListRequestHandler(proxyId, callId);
            else if (payload.Is(OpenOrderRequest.Descriptor))
                return OpenOrderRequestHandler(proxyId, payload);
            else if (payload.Is(ModifyOrderRequest.Descriptor))
                return ModifyOrderRequestHandler(proxyId, payload);
            else if (payload.Is(CloseOrderRequest.Descriptor))
                return CloseOrderRequestHandler(proxyId, payload);
            else if (payload.Is(CancelOrderRequest.Descriptor))
                return CancelOrderRequestHandler(proxyId, payload);
            else if (payload.Is(TradeHistoryRequest.Descriptor))
                return TradeHistoryRequestHandler(proxyId, callId, payload);
            else if (payload.Is(TradeHistoryRequestNextPage.Descriptor))
                return TradeHistoryRequestNextPageHandler(callId);
            else if (payload.Is(TradeHistoryRequestDispose.Descriptor))
                return TradeHistoryRequestDisposeHandler(callId);
            else if (payload.Is(FeedSnapshotRequest.Descriptor))
                return FeedSnapshotRequestHandler(proxyId);
            else if (payload.Is(ModifyFeedSubscriptionRequest.Descriptor))
                return ModifyFeedSubscriptionRequestHandler(proxyId, payload);
            else if (payload.Is(CancelAllFeedSubscriptionsRequest.Descriptor))
                return CancelAllFeedSubscriptionsRequestHandler(proxyId);
            else if (payload.Is(BarListRequest.Descriptor))
                return BarListRequestHandler(proxyId, payload);
            else if (payload.Is(QuoteListRequest.Descriptor))
                return QuoteListRequestHandler(proxyId, payload);

            return Task.FromResult(default(Any));
        }


        private Task<Any> RuntimeConfigRequestHandler()
        {
            return Task.FromResult(Any.Pack(_runtime.Config));
        }

        private Task<Any> PackagePathRequestHandler(Any payload)
        {
            //var request = payload.Unpack<PackagePathRequest>();
            return Task.FromResult(Any.Pack(new PackagePathResponse { Path = _runtime.GetPackagePath() }));
        }

        private Task<Any> ExecutorConfigRequestHandler(string executorId)
        {
            if (!_server.TryGetExecutor(executorId, out var executor))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown executor" }));

            return Task.FromResult(Any.Pack(executor.Config));
        }

        private Task<Any> AttachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<AttachAccountRequest>();
            _runtime.AttachAccount(request.AccountId);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> DetachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<DetachAccountRequest>();
            _runtime.DetachAccount(request.AccountId);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> ConnectionInfoRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            return Task.FromResult(Any.Pack(new ConnectionInfoResponse { ConnectionInfo = account.Id }));
        }

        private Task<Any> CurrencyListRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new CurrencyListResponse();
            response.Currencies.Add(account.Metadata.GetCurrencyMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> SymbolListRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new SymbolListResponse();
            response.Symbols.Add(account.Metadata.GetSymbolMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> LastQuoteListRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new LastQuoteListResponse();
            response.Quotes.Add(account.Metadata.GetLastQuoteMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> AccountInfoRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new AccountInfoResponse();
            response.Account = account.AccInfoProvider.GetAccountInfo();
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> OrderListRequestHandler(string accountId, string callId)
        {
            const int chunkSize = 10;

            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new OrderListResponse { IsFinal = false };
            var orders = account.AccInfoProvider.GetOrders();
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

        private Task<Any> PositionListRequestHandler(string accountId, string callId)
        {
            const int chunkSize = 10;

            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new PositionListResponse { IsFinal = false };
            var positions = account.AccInfoProvider.GetPositions();
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

        private Task<Any> OpenOrderRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<OpenOrderRequest>();
            account.TradeExecutor.SendOpenOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> ModifyOrderRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<ModifyOrderRequest>();
            account.TradeExecutor.SendModifyOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> CloseOrderRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<CloseOrderRequest>();
            account.TradeExecutor.SendCloseOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> CancelOrderRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<CancelOrderRequest>();
            account.TradeExecutor.SendCancelOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private void PluginLogRecordHandler(string executorId, Any payload)
        {
            if (!_server.TryGetExecutor(executorId, out var executor))
                return;

            var record = payload.Unpack<PluginLogRecord>();
            executor.OnLogUpdated(record);
        }

        private void PluginErrorHandler(string executorId, Any payload)
        {
            if (!_server.TryGetExecutor(executorId, out var executor))
                return;

            var error = payload.Unpack<PluginError>();
            executor.OnErrorOccured(new AlgoPluginException(error));
        }

        private void PluginStoppedHandler(string executorId)
        {
            if (!_server.TryGetExecutor(executorId, out var executor))
                return;

            executor.OnStopped();
        }

        private void DataSeriesUpdateHandler(string executorId, Any payload)
        {
            if (!_server.TryGetExecutor(executorId, out var executor))
                return;

            var update = payload.Unpack<DataSeriesUpdate>();
            executor.OnDataSeriesUpdate(update);
        }

        private Task<Any> TradeHistoryRequestHandler(string accountId, string callId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<TradeHistoryRequest>();
            var enumerator = account.TradeHistoryProvider.GetTradeHistory(request.From?.ToDateTime(), request.To?.ToDateTime(), request.Options);
            if (enumerator != null)
            {
                _pendingRequestHandlers.TryAdd(callId, enumerator);
            }
            return Task.FromResult<Any>(null);
        }

        private Task<Any> TradeHistoryRequestNextPageHandler(string callId)
        {
            _pendingRequestHandlers.TryGetValue(callId, out var state);
            var enumerator = (IAsyncPagedEnumerator<TradeReportInfo>)state;
            var page = enumerator.GetNextPage().GetAwaiter().GetResult();
            var response = new TradeHistoryPageResponse();
            if (page == null || page.Length == 0)
            {
                _pendingRequestHandlers.TryRemove(callId, out state);
                enumerator.Dispose();
            }
            else
            {
                response.Reports.AddRange(page);
            }
            return Task.FromResult(Any.Pack(response));
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

        private Task<Any> FeedSnapshotRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var response = new QuotePage();
            response.Quotes.AddRange(account.Feed.GetSnapshot().Select(q => q.GetFullQuote()));
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> ModifyFeedSubscriptionRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            var request = payload.Unpack<ModifyFeedSubscriptionRequest>();
            var response = new QuotePage();
            var snapshot = account.Feed.Sync.Invoke(() => account.Feed.Modify(request.Updates.ToList()));
            response.Quotes.AddRange(snapshot.Select(q => q.GetFullQuote()));
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> CancelAllFeedSubscriptionsRequestHandler(string accountId)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Task.FromResult(Any.Pack(new ErrorResponse { Message = "Unknown account" }));

            account.Feed.Sync.Invoke(() => account.Feed.CancelAll());
            return Task.FromResult(VoidResponse);
        }

        private async Task<Any> BarListRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Any.Pack(new ErrorResponse { Message = "Unknown account" });

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
                ? account.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, count.Value)
                : account.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, request.To));
            response.Bars.AddRange(barList);

            return Any.Pack(response);
        }

        private async Task<Any> QuoteListRequestHandler(string accountId, Any payload)
        {
            if (!_server.TryGetAccount(accountId, out var account))
                return Any.Pack(new ErrorResponse { Message = "Unknown account" });

            var request = payload.Unpack<QuoteListRequest>();
            var symbol = request.Symbol;
            var count = request.Count;
            var response = new QuoteChunk { Symbol = symbol, };
            var quoteList = await (count.HasValue
                ? account.FeedHistory.QueryQuotesAsync(symbol, request.From, count.Value, request.Level2)
                : account.FeedHistory.QueryQuotesAsync(symbol, request.From, request.To, request.Level2));
            response.Quotes.AddRange(quoteList.Select(q => q.GetData()));

            return Any.Pack(response);
        }
    }
}

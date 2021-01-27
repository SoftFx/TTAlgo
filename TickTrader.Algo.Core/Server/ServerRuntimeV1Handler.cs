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

        public void HandleNotification(string callId, Any payload)
        {
            if (payload.Is(UnitLogRecord.Descriptor))
                UnitLogRecordHandler(payload);
            else if (payload.Is(UnitError.Descriptor))
                UnitErrorHandler(payload);
            else if (payload.Is(UnitStopped.Descriptor))
                UnitStoppedHandler();
            else if (payload.Is(DataSeriesUpdate.Descriptor))
                DataSeriesUpdateHandler(payload);

        }

        public Task<Any> HandleRequest(string callId, Any payload)
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

                    var proxy = new RuntimeProxy(_session);
                    _runtime.OnAttached(r => _session.Tell(r), proxy);

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
            else if (payload.Is(ConnectionInfoRequest.Descriptor))
                return ConnectionInfoRequestHandler();
            else if (payload.Is(CurrencyListRequest.Descriptor))
                return CurrencyListRequestHandler();
            else if (payload.Is(SymbolListRequest.Descriptor))
                return SymbolListRequestHandler();
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
            else if (payload.Is(FeedSnapshotRequest.Descriptor))
                return FeedSnapshotRequestHandler();
            else if (payload.Is(ModifyFeedSubscriptionRequest.Descriptor))
                return ModifyFeedSubscriptionRequestHandler(payload);
            else if (payload.Is(CancelAllFeedSubscriptionsRequest.Descriptor))
                return CancelAllFeedSubscriptionsRequestHandler();
            else if (payload.Is(BarListRequest.Descriptor))
                return BarListRequestHandler(payload);
            else if (payload.Is(QuoteListRequest.Descriptor))
                return QuoteListRequestHandler(payload);
            return null;
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

        private Task<Any> ConnectionInfoRequestHandler()
        {
            return Task.FromResult(Any.Pack(new ConnectionInfoResponse { ConnectionInfo = _runtime.ConnectionInfo }));
        }

        private Task<Any> CurrencyListRequestHandler()
        {
            var response = new CurrencyListResponse();
            response.Currencies.Add(_runtime.Metadata.GetCurrencyMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> SymbolListRequestHandler()
        {
            var response = new SymbolListResponse();
            response.Symbols.Add(_runtime.Metadata.GetSymbolMetadata());
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> AccountInfoRequestHandler()
        {
            var response = new AccountInfoResponse();
            response.Account = _runtime.AccInfoProvider.GetAccountInfo();
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> OrderListRequestHandler(string callId)
        {
            const int chunkSize = 10;

            var response = new OrderListResponse { IsFinal = false };
            var orders = _runtime.AccInfoProvider.GetOrders();
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
            var positions = _runtime.AccInfoProvider.GetPositions();
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
            _runtime.TradeExecutor.SendOpenOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> ModifyOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyOrderRequest>();
            _runtime.TradeExecutor.SendModifyOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> CloseOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CloseOrderRequest>();
            _runtime.TradeExecutor.SendCloseOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private Task<Any> CancelOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CancelOrderRequest>();
            _runtime.TradeExecutor.SendCancelOrder(request);
            return Task.FromResult(VoidResponse);
        }

        private void UnitLogRecordHandler(Any payload)
        {
            var record = payload.Unpack<UnitLogRecord>();
            _runtime.OnLogUpdated(record);
        }

        private void UnitErrorHandler(Any payload)
        {
            var error = payload.Unpack<UnitError>();
            _runtime.OnErrorOccured(new AlgoUnitException(error));
        }

        private void UnitStoppedHandler()
        {
            _runtime.OnStopped();
        }

        private void DataSeriesUpdateHandler(Any payload)
        {
            var update = payload.Unpack<DataSeriesUpdate>();
            _runtime.OnDataSeriesUpdate(update);
        }

        private Task<Any> TradeHistoryRequestHandler(string callId, Any payload)
        {
            var request = payload.Unpack<TradeHistoryRequest>();
            var enumerator = _runtime.TradeHistoryProvider.GetTradeHistory(request.From?.ToDateTime(), request.To?.ToDateTime(), request.Options);
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

        private Task<Any> FeedSnapshotRequestHandler()
        {
            var response = new QuotePage();
            response.Quotes.AddRange(_runtime.Feed.GetSnapshot().Select(q => q.GetFullQuote()));
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> ModifyFeedSubscriptionRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyFeedSubscriptionRequest>();
            var response = new QuotePage();
            var snapshot = _runtime.Feed.Sync.Invoke(() => _runtime.Feed.Modify(request.Updates.ToList()));
            response.Quotes.AddRange(snapshot.Select(q => q.GetFullQuote()));
            return Task.FromResult(Any.Pack(response));
        }

        private Task<Any> CancelAllFeedSubscriptionsRequestHandler()
        {
            _runtime.Feed.Sync.Invoke(() => _runtime.Feed.CancelAll());
            return Task.FromResult(VoidResponse);
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
                ? _runtime.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, count.Value)
                : _runtime.FeedHistory.QueryBarsAsync(symbol, marketSide, timeframe, request.From, request.To));
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
                ? _runtime.FeedHistory.QueryQuotesAsync(symbol, request.From, count.Value, request.Level2)
                : _runtime.FeedHistory.QueryQuotesAsync(symbol, request.From, request.To, request.Level2));
            response.Quotes.AddRange(quoteList.Select(q => q.GetData()));

            return Any.Pack(response);
        }
    }
}

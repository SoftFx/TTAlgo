using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class ServerRuntimeV1Handler : IRpcHandler
    {
        private static readonly Any VoidResponse = Any.Pack(new VoidResponse());

        private readonly AlgoServer _server;
        private PluginExecutor _executor;
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

        }

        public Any HandleRequest(string callId, Any payload)
        {
            if (payload.Is(AttachPluginRequest.Descriptor))
            {
                var request = payload.Unpack<AttachPluginRequest>();
                if (_executor != null)
                {
                    return Any.Pack(new ErrorResponse { Message = "Executor already attached!" });
                }
                if (_server.TryGetExecutor(request.Id, out var executor))
                {
                    _executor = executor;
                    _executor.AccInfoProvider.OrderUpdated += r => _session.Tell(RpcMessage.Notification(r));
                    _executor.AccInfoProvider.PositionUpdated += r => _session.Tell(RpcMessage.Notification(r));
                    _executor.AccInfoProvider.BalanceUpdated += r => _session.Tell(RpcMessage.Notification(r));
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            _executor.Start();
                        }
                        catch (Exception) { }
                    });
                    return Any.Pack(new AttachPluginResponse { Success = true });
                }
                else
                {
                    return Any.Pack(new AttachPluginResponse { Success = false });
                }
            }
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
            return null;
        }


        private Any CurrencyListRequestHandler()
        {
            var response = new CurrencyListResponse();
            response.Currencies.Add(
                _executor.Metadata.GetCurrencyMetadata()
                .Select(c => c.Info));
            return Any.Pack(response);
        }

        private Any SymbolListRequestHandler()
        {
            var response = new SymbolListResponse();
            response.Symbols.Add(_executor.Metadata.GetSymbolMetadata());
            return Any.Pack(response);
        }

        private Any AccountInfoRequestHandler()
        {
            var response = new AccountInfoResponse();
            response.Account = _executor.AccInfoProvider.GetAccountInfo();
            return Any.Pack(response);
        }

        private Any OrderListRequestHandler(string callId)
        {
            const int chunkSize = 10;

            var response = new OrderListResponse { IsFinal = false };
            var orders = _executor.AccInfoProvider.GetOrders();
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
            return Any.Pack(response);
        }

        private Any PositionListRequestHandler(string callId)
        {
            const int chunkSize = 10;

            var response = new PositionListResponse { IsFinal = false };
            var positions = _executor.AccInfoProvider.GetPositions();
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
            return Any.Pack(response);
        }

        private Any OpenOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<OpenOrderRequest>();
            _executor.TradeExecutor.SendOpenOrder(request);
            return VoidResponse;
        }

        private Any ModifyOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyOrderRequest>();
            _executor.TradeExecutor.SendModifyOrder(request);
            return VoidResponse;
        }

        private Any CloseOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CloseOrderRequest>();
            _executor.TradeExecutor.SendCloseOrder(request);
            return VoidResponse;
        }

        private Any CancelOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CancelOrderRequest>();
            _executor.TradeExecutor.SendCancelOrder(request);
            return VoidResponse;
        }

        private void UnitLogRecordHandler(Any payload)
        {
            var record = payload.Unpack<UnitLogRecord>();
            _executor.OnLogUpdated(record);
        }

        private void UnitErrorHandler(Any payload)
        {
            var error = payload.Unpack<UnitError>();
            _executor.OnErrorOccured(new AlgoUnitException(error));
        }

        private void UnitStoppedHandler()
        {
            _executor.OnStopped();
        }

        private Any TradeHistoryRequestHandler(string callId, Any payload)
        {
            var request = payload.Unpack<TradeHistoryRequest>();
            var enumerator = _executor.TradeHistoryProvider.GetTradeHistory(request.From?.ToDateTime(), request.To?.ToDateTime(), request.Options);
            if (enumerator != null)
            {
                _pendingRequestHandlers.TryAdd(callId, enumerator);
            }
            return null;
        }

        private Any TradeHistoryRequestNextPageHandler(string callId)
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
            return Any.Pack(response);
        }

        private Any TradeHistoryRequestDisposeHandler(string callId)
        {
            if (_pendingRequestHandlers.TryRemove(callId, out var state))
            {
                var enumerator = (IAsyncPagedEnumerator<TradeReportInfo>)state;
                enumerator.Dispose();
            }
            return null;
        }
    }
}

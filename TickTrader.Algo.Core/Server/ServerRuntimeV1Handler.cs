using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class ServerRuntimeV1Handler : IRpcHandler
    {
        private readonly AlgoServer _server;
        private PluginExecutor _executor;
        private RpcSession _session;


        public ServerRuntimeV1Handler(AlgoServer server)
        {
            _server = server;
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
        }

        public void HandleNotification(string callId, Any payload)
        {
            if (payload.Is(OpenOrderRequest.Descriptor))
                OpenOrderRequestHandler(payload);
            else if (payload.Is(ModifyOrderRequest.Descriptor))
                ModifyOrderRequestHandler(payload);
            else if (payload.Is(CloseOrderRequest.Descriptor))
                CloseOrderRequestHandler(payload);
            else if (payload.Is(CancelOrderRequest.Descriptor))
                CancelOrderRequestHandler(payload);
        }

        public Any HandleRequest(string callId, Any payload)
        {
            if (payload.Is(AttachPluginRequest.Descriptor))
            {
                var request = payload.Unpack<AttachPluginRequest>();
                if (_executor != null)
                {
                    return Any.Pack(new RpcError { Message = "Executor already attached!" });
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

        private void OpenOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<OpenOrderRequest>();
            var callback = new CrossDomainCallback<Domain.OrderExecReport.Types.CmdResultCode> { Action = _ => { } };
            _executor.TradeExecutor.SendOpenOrder(callback, request);
        }

        private void ModifyOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<ModifyOrderRequest>();
            var callback = new CrossDomainCallback<Domain.OrderExecReport.Types.CmdResultCode> { Action = _ => { } };
            _executor.TradeExecutor.SendModifyOrder(callback, request);
        }

        private void CloseOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CloseOrderRequest>();
            var callback = new CrossDomainCallback<Domain.OrderExecReport.Types.CmdResultCode> { Action = _ => { } };
            _executor.TradeExecutor.SendCloseOrder(callback, request);
        }

        private void CancelOrderRequestHandler(Any payload)
        {
            var request = payload.Unpack<CancelOrderRequest>();
            var callback = new CrossDomainCallback<Domain.OrderExecReport.Types.CmdResultCode> { Action = _ => { } };
            _executor.TradeExecutor.SendCancelOrder(callback, request);
        }
    }
}

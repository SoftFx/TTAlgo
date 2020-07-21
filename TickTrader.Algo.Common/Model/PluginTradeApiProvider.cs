using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class PluginTradeApiProvider : ActorPart
    {
        private ConnectionModel _connection;

        private bool IsConnected => _connection.State == ConnectionModel.States.Online;

        internal event Action<ExecutionReport> OnExclusiveReport;

        internal PluginTradeApiProvider(ConnectionModel connection)
        {
            _connection = connection;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendCancelOrder(Domain.CancelOrderRequest request)
        {
            if (!IsConnected)
            {
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;
            }

            var result = await _connection.TradeProxy.SendCancelOrder(request);
            SendReports(request, result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendCloseOrder(Domain.CloseOrderRequest request)
        {
            if (!IsConnected)
            {
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;
            }

            var result = await _connection.TradeProxy.SendCloseOrder(request);
            SendReports(request, result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendModifyOrder(Domain.ModifyOrderRequest request)
        {
            if (!IsConnected)
            {
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;
            }

            var result = await _connection.TradeProxy.SendModifyOrder(request);
            SendReports(request, result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendOpenOrder(Domain.OpenOrderRequest request)
        {
            if (!IsConnected)
            {
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;
            }

            var result = await _connection.TradeProxy.SendOpenOrder(request);
            SendReports(request, result);
            return result.ResultCode;
        }

        private void SendReports(ITradeRequest request, OrderInteropResult result)
        {
            if (result.Reports != null)
            {
                foreach(var rep in result.Reports)
                    OnExclusiveReport?.Invoke(rep);
            }
            if (result.ResultCode != Domain.OrderExecReport.Types.CmdResultCode.Ok)
            {
                SendRejectReport(request.OperationId, result.ResultCode);
            }
        }

        private void SendRejectReport(string operationId, Domain.OrderExecReport.Types.CmdResultCode rejectCode)
        {
            var rep = new ExecutionReport
            {
                ClientOrderId = operationId,
                OrderStatus = OrderStatus.Rejected,
                RejectReason = rejectCode,
            };
            OnExclusiveReport?.Invoke(rep);
        }

        public class Handler : CrossDomainObject, ITradeExecutor
        {
            private Ref<PluginTradeApiProvider> _ref;

            internal Handler(Ref<PluginTradeApiProvider> aRef)
            {
                _ref = aRef;
            }

            public async void SendCancelOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.CancelOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendCancelOrder(request));
                callback.Invoke(result);
            }

            public async void SendCloseOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.CloseOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendCloseOrder(request));
                callback.Invoke(result);
            }

            public async void SendModifyOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.ModifyOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendModifyOrder(request));
                callback.Invoke(result);
            }

            public async void SendOpenOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.OpenOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendOpenOrder(request));
                callback.Invoke(result);
            }
        }
    }
}

using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
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

        private async Task SendCancelOrder(Domain.CancelOrderRequest request)
        {
            if (!IsConnected)
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);

            var result = await _connection.TradeProxy.SendCancelOrder(request);
            SendReports(request, result);
        }

        private async Task SendCloseOrder(Domain.CloseOrderRequest request)
        {
            if (!IsConnected)
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);

            var result = await _connection.TradeProxy.SendCloseOrder(request);
            SendReports(request, result);
        }

        private async Task SendModifyOrder(Domain.ModifyOrderRequest request)
        {
            if (!IsConnected)
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);

            var result = await _connection.TradeProxy.SendModifyOrder(request);
            SendReports(request, result);
        }

        private async Task SendOpenOrder(Domain.OpenOrderRequest request)
        {
            if (!IsConnected)
                SendRejectReport(request.OperationId, Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);

            var result = await _connection.TradeProxy.SendOpenOrder(request);
            SendReports(request, result);
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
                ExecutionType = ExecutionType.Rejected,
                ClientOrderId = operationId,
                OrderStatus = OrderStatus.Rejected,
                RejectReason = rejectCode,
            };
            OnExclusiveReport?.Invoke(rep);
        }

        public class Handler : ITradeExecutor
        {
            private Ref<PluginTradeApiProvider> _ref;

            internal Handler(Ref<PluginTradeApiProvider> aRef)
            {
                _ref = aRef;
            }

            public async void SendCancelOrder(Domain.CancelOrderRequest request)
            {
                await _ref.Call(a => a.SendCancelOrder(request));
            }

            public async void SendCloseOrder(Domain.CloseOrderRequest request)
            {
                await _ref.Call(a => a.SendCloseOrder(request));
            }

            public async void SendModifyOrder(Domain.ModifyOrderRequest request)
            {
                await _ref.Call(a => a.SendModifyOrder(request));
            }

            public async void SendOpenOrder(Domain.OpenOrderRequest request)
            {
                await _ref.Call(a => a.SendOpenOrder(request));
            }
        }
    }
}

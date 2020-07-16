using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

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

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendCancelOrder(CancelOrderRequest request)
        {
            if (!IsConnected)
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;

            var result = await _connection.TradeProxy.SendCancelOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendCloseOrder(CloseOrderCoreRequest request)
        {
            if (!IsConnected)
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;

            var result = await _connection.TradeProxy.SendCloseOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendModifyOrder(ReplaceOrderCoreRequest request)
        {
            if (!IsConnected)
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;

            var result = await _connection.TradeProxy.SendModifyOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<Domain.OrderExecReport.Types.CmdResultCode> SendOpenOrder(OpenOrderCoreRequest request)
        {
            if (!IsConnected)
                return Domain.OrderExecReport.Types.CmdResultCode.ConnectionError;

            var result = await _connection.TradeProxy.SendOpenOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private void SendReports(OrderInteropResult result)
        {
            if (result.Reports != null)
            {
                foreach(var rep in result.Reports)
                    OnExclusiveReport?.Invoke(rep);
            }
        }

        public class Handler : CrossDomainObject, ITradeExecutor
        {
            private Ref<PluginTradeApiProvider> _ref;

            internal Handler(Ref<PluginTradeApiProvider> aRef)
            {
                _ref = aRef;
            }

            public async void SendCancelOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CancelOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendCancelOrder(request));
                callback.Invoke(result);
            }

            public async void SendCloseOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CloseOrderCoreRequest request)
            {
                var result = await _ref.Call(a => a.SendCloseOrder(request));
                callback.Invoke(result);
            }

            public async void SendModifyOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, ReplaceOrderCoreRequest request)
            {
                var result = await _ref.Call(a => a.SendModifyOrder(request));
                callback.Invoke(result);
            }

            public async void SendOpenOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, OpenOrderCoreRequest request)
            {
                var result = await _ref.Call(a => a.SendOpenOrder(request));
                callback.Invoke(result);
            }
        }
    }
}

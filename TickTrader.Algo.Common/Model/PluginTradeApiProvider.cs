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

        private async Task<OrderCmdResultCodes> SendCancelOrder(CancelOrderRequest request)
        {
            if (!IsConnected)
                return OrderCmdResultCodes.ConnectionError;

            var result = await _connection.TradeProxy.SendCancelOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<OrderCmdResultCodes> SendCloseOrder(CloseOrderRequest request)
        {
            if (!IsConnected)
                return OrderCmdResultCodes.ConnectionError;

            var result = await _connection.TradeProxy.SendCloseOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<OrderCmdResultCodes> SendModifyOrder(ReplaceOrderRequest request)
        {
            if (!IsConnected)
                return OrderCmdResultCodes.ConnectionError;

            var result = await _connection.TradeProxy.SendModifyOrder(request);
            SendReports(result);
            return result.ResultCode;
        }

        private async Task<OrderCmdResultCodes> SendOpenOrder(OpenOrderRequest request)
        {
            if (!IsConnected)
                return OrderCmdResultCodes.ConnectionError;

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

            public async void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendCancelOrder(request));
                callback.Invoke(result);
            }

            public async void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendCloseOrder(request));
                callback.Invoke(result);
            }

            public async void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendModifyOrder(request));
                callback.Invoke(result);
            }

            public async void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request)
            {
                var result = await _ref.Call(a => a.SendOpenOrder(request));
                callback.Invoke(result);
            }
        }
    }
}

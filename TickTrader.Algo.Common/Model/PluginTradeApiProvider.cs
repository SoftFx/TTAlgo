using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class PluginTradeApiProvider : CrossDomainObject, ITradeExecutor
    {
        private ConnectionModel _connection;

        public PluginTradeApiProvider(ConnectionModel connection)
        {
            _connection = connection;
        }

        public void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request)
        {
            _connection.TradeProxy.SendCancelOrder(request)
                .ContinueWith(t => callback.Invoke(t.Result));
        }

        public void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request)
        {
            _connection.TradeProxy.SendCloseOrder(request)
                .ContinueWith(t => callback.Invoke(t.Result));
        }

        public void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request)
        {
            _connection.TradeProxy.SendModifyOrder(request)
                .ContinueWith(t => callback.Invoke(t.Result));
        }

        public void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request)
        {
            _connection.TradeProxy.SendOpenOrder(request)
                .ContinueWith(t => callback.Invoke(t.Result));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi api;
        private SymbolProvider symbols;
        private AccountDataProvider account;

        public TradeApiAdapter(ITradeApi api, SymbolProvider symbols, AccountDataProvider account)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
        }

        public Task<OrderCmdResult> OpenOrder(string symbol, OrderType type, OrderSide side, double price, double volumeLots, double? tp, double? sl, string comment)
        {
            OrderCmdResultCodes code;
            double volume = ConvertVolume(volumeLots, symbol, out code);
            if (code != OrderCmdResultCodes.Ok)
                return CreateResult(code);

            var waitHandler = new TaskProxy<OrderCmdResult>();
            api.OpenOrder(waitHandler, symbol, type, side, price, volume, tp, sl, comment);
            return waitHandler.LocalTask;
        }

        public Task<OrderCmdResult> CancelOrder(string orderId)
        {
            Order orderToCancel = account.Orders[orderId];
            if (orderToCancel.IsNull)
                return CreateResult(OrderCmdResultCodes.OrderNotFound);

            var waitHandler = new TaskProxy<OrderCmdResult>();
            api.CancelOrder(waitHandler, orderId, orderToCancel.Side);
            return waitHandler.LocalTask;
        }

        public Task<OrderCmdResult> CloseOrder(string orderId, double? closeVolumeLots)
        {            
            double? closeVolume = null;

            if (closeVolumeLots != null)
            {
                Order orderToClose = account.Orders[orderId];
                if (orderToClose.IsNull)
                    return CreateResult(OrderCmdResultCodes.OrderNotFound);

                OrderCmdResultCodes code;
                closeVolume = ConvertVolume(closeVolumeLots.Value, orderToClose.Symbol, out code);
                if (code != OrderCmdResultCodes.Ok)
                    return CreateResult(code);
            }

            var waitHandler = new TaskProxy<OrderCmdResult>();
            api.CloseOrder(waitHandler, orderId, closeVolume);
            return waitHandler.LocalTask;
        }

        public Task<OrderCmdResult> ModifyOrder(string orderId, double price, double? tp, double? sl, string comment)
        {
            Order orderToModify = account.Orders[orderId];
            if (orderToModify.IsNull)
                return CreateResult(OrderCmdResultCodes.OrderNotFound);

            var waitHandler = new TaskProxy<OrderCmdResult>();
            api.ModifyOrder(waitHandler, orderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                price, orderToModify.RequestedAmount, tp, sl, comment);
            return waitHandler.LocalTask;
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new TradeResultEntity(code));
        }

        private double ConvertVolume(double volumeInLots, string symbolCode, out OrderCmdResultCodes rCode)
        {
            var smbMetatda = symbols.List[symbolCode];
            if (smbMetatda.IsNull)
            {
                rCode = OrderCmdResultCodes.SymbolNotFound;
                return double.NaN;
            }
            else
            {
                rCode = OrderCmdResultCodes.Ok;
                return smbMetatda.ContractSize * volumeInLots;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : ITradeCommands
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

        public Task<OrderCmdResult> CancelOrder(CancelOrdeRequest request)
        {
            return api.CancelOrder(request);
        }

        public Task<OrderCmdResult> CloseOrder(CloseOrdeRequest request)
        {
            return api.CloseOrder(request);
        }

        public Task<OrderCmdResult> ModifyOrder(ModifyOrdeRequest request)
        {
            return api.ModifyOrder(request);
        }

        public Task<OrderCmdResult> OpenMarketOrder(string symbolCode, OrderSides side, OrderVolume volume, double? stopLoss = default(double?), double? takeProfit = default(double?), string comment = null)
        {
            OrderCmdResultCodes code;
            double vol = ResolveVolume(volume, symbolCode, out code);
            if (code != OrderCmdResultCodes.Ok)
                return CreateResult(code);

            var request = new OpenOrdeRequest()
            {
                OrderType = OrderTypes.Market,
                Side = side,
                Volume = vol,
                SymbolCode = symbolCode,
                Comment = comment,
                Price = 1,
                StopLoss = stopLoss,
                TaskProfit = takeProfit
            };

            return OpenOrder(request);
        }

        public Task<OrderCmdResult> OpenOrder(OpenOrdeRequest request)
        {
            var waitHandler = new TaskProxy<OrderCmdResult>();
            api.OpenOrder(request, waitHandler);
            return waitHandler.LocalTask;
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new TradeResultEntity(code));
        }

        private double ResolveVolume(OrderVolume volume, string symbolCode, out OrderCmdResultCodes rCode)
        {
            if (volume.Units == VolumeUnits.CurrencyUnits)
            {
                rCode = OrderCmdResultCodes.Ok;
                return volume.Value;
            }
            else // if (volume.Units == VolumeUnits.Lots)
            {
                var smbMetatda = symbols[symbolCode];
                if (smbMetatda.IsNull)
                {
                    rCode = OrderCmdResultCodes.SymbolNotFound;
                    return -1;
                }
                else
                {
                    rCode = OrderCmdResultCodes.Ok;
                    return smbMetatda.LotSize * volume.Value;
                }
            }
        }
    }
}

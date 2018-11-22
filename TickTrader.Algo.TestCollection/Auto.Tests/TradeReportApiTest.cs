using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    internal class TradeReportApiTest : AutoTest
    {
        private List<Order> _openedOrders = new List<Order>();

        public override string Name => throw new NotImplementedException();

        public override async Task RunTest()
        {
            await OpenMarket(OrderSide.Buy, 2);
            await OpenMarket(OrderSide.Sell, 1);
            await OpenMarket(OrderSide.Sell, 1);

            await TradeApi.Delay(2000);

            foreach (var report in TradeApi.Account.TradeHistory)
            {
                TradeApi.Print("report " + report.ReportId + " order #" + report.OrderId + " " + report.ActionType);
            }
        }

        private async Task OpenMarket(OrderSide side, double volumeFactor)
        {
            var volume = BaseOrderVolume * volumeFactor;

            var resp = await TradeApi.OpenOrderAsync(OrderSymbol, OrderType.Market, side, volume, null, null, null);

            if (resp.IsFaulted)
                throw new Exception("Failed to open order - " + resp.ResultingOrder);

            if (AccType == AccountTypes.Gross)
            {
                var closeResp = await TradeApi.CloseOrderAsync(resp.ResultingOrder.Id);

                if (closeResp.IsFaulted)
                    throw new Exception("Failed to close order - " + resp.ResultingOrder);
            }
        }

        private void CheckTradeReport()
        {
        }
    }
}

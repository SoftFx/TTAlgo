using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "Account Info Display Bot")]
    public class AccDisplayBot : TradeBot
    {
        protected override void OnStart()
        {
            PrintStat();

            Account.Orders.Opened += args => PrintStat();
            Account.Orders.Closed += args => PrintStat();
            Account.Orders.Canceled += args => PrintStat();
            Account.Orders.Modified += args => PrintStat();
            Account.Orders.Filled += args => PrintStat();
            Account.Orders.Expired += args => PrintStat();
            Account.Assets.Modified += args => PrintStat();
            Account.BalanceUpdated += () => PrintStat();
        }

        private void PrintStat()
        {
            PrintAccountInfo();

            if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
                PrintBalance();

            Status.WriteLine();

            PrintPendingOrders();

            Status.WriteLine();

            if (Account.Type == AccountTypes.Gross)
                PrintGrossPositions();
            else if (Account.Type == AccountTypes.Net)
                PrintNetPositions();
            else if (Account.Type == AccountTypes.Cash)
                PrintAssets();
        }

        private void PrintAccountInfo()
        {
            Status.WriteLine("{0} {1}", Account.Id, Account.Type);
        }

        private void PrintPendingOrders()
        {
            var pOrders = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();
            if (pOrders.Count > 0)
            {
                Status.WriteLine("{0}  orders:", pOrders.Count);
                foreach (var order in pOrders)
                {
                    Status.WriteLine("#{0} {1} {2}/{3} {4}", order.Symbol, order.Side,
                        order.RequestedVolume, order.RequestedVolume, order.Comment);
                }
            }
            else
                Status.WriteLine("No orders");
        }

        private void PrintGrossPositions()
        {
            var positions = Account.Orders.Where(o => o.Type == OrderType.Position).ToList();
            if (positions.Count > 0)
            {
                Status.WriteLine("{0} positions:", positions.Count);
                foreach (var order in positions)
                {
                    Status.WriteLine("#{0} {1} {2} {3}/{4} {5}", order.Id, order.Symbol, order.Side,
                        order.RequestedVolume, order.RequestedVolume, order.Comment);
                }
            }
            else
                Status.WriteLine("No positions");
        }

        private void PrintNetPositions()
        {
        }

        private void PrintAssets()
        {
            if (Account.Assets.Count > 0)
            {
                Status.WriteLine("{0} assets:", Account.Assets.Count);
                foreach (var asset in Account.Assets)
                    Status.WriteLine("#{0} {1}", asset.Currency, asset.Volume);
            }
            else
                Status.WriteLine("No assets");
        }

        private void PrintBalance()
        {
            Status.WriteLine("{0} {1}", Account.Balance, Account.BalanceCurrency);
        }
    }
}

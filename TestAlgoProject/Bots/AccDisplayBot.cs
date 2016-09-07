using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
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
            StringBuilder builder = new StringBuilder();

            PrintAccountInfo(builder);

            if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
                PrintBalance(builder);

            builder.AppendLine();

            PrintPendingOrders(builder);

            builder.AppendLine();

            if (Account.Type == AccountTypes.Gross)
                PrintGrossPositions(builder);
            else if (Account.Type == AccountTypes.Net)
                PrintNetPositions(builder);
            else if (Account.Type == AccountTypes.Cash)
                PrintAssets(builder);

            UpdateStatus(builder.ToString());
        }

        private void PrintAccountInfo(StringBuilder builder)
        {
            builder.Append(Account.Id).Append(" ").Append(Account.Type).AppendLine();
        }

        private void PrintPendingOrders(StringBuilder builder)
        {
            var pOrders = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();
            if (pOrders.Count > 0)
            {
                builder.Append(pOrders.Count).AppendLine(" orders:");
                foreach (var order in pOrders)
                {
                    builder.Append("#").Append(order.Id)
                        .Append(" ").Append(order.Symbol)
                        .Append(" ").Append(order.Side)
                        .Append(" ").Append(order.RemainingAmount).Append('/').Append(order.RequestedAmount)
                        .AppendLine();
                }
            }
            else
                builder.AppendLine("No orders");
        }

        private void PrintGrossPositions(StringBuilder builder)
        {
            var positions = Account.Orders.Where(o => o.Type == OrderType.Position).ToList();
            if (positions.Count > 0)
            {
                builder.Append(positions.Count).AppendLine(" positions:");
                foreach (var order in positions)
                {
                    builder.Append("#").Append(order.Id)
                        .Append(" ").Append(order.Symbol)
                        .Append(" ").Append(order.Side)
                        .Append(" ").Append(order.RemainingAmount).Append('/').Append(order.RequestedAmount)
                        .AppendLine();
                }
            }
            else
                builder.AppendLine("No positions");
        }

        private void PrintNetPositions(StringBuilder builder)
        {
        }

        private void PrintAssets(StringBuilder builder)
        {
            if (Account.Assets.Count > 0)
            {
                builder.Append(Account.Assets.Count).AppendLine(" assets:");
                foreach (var asset in Account.Assets)
                {
                    builder.Append("#").Append(asset.CurrencyCode)
                        .Append(" ").Append(asset.Volume)
                        .AppendLine();
                }
            }
            else
                builder.AppendLine("No assets");
        }

        private void PrintBalance(StringBuilder builder)
        {
            builder.Append(Account.Balance).Append(" ").Append(Account.BalanceCurrency).AppendLine();
        }
    }
}

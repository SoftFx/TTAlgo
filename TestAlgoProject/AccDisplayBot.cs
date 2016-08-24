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
        }

        private void PrintStat()
        {
            StringBuilder builder = new StringBuilder();
            PrintPendingOrders(builder);
            PrintPositions(builder);
            UpdateStatus(builder.ToString());
        }

        private void PrintPendingOrders(StringBuilder builder)
        {
            var pOrders = Account.Orders.Where(o => o.Type != OrderTypes.Position).ToList();
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

        private void PrintPositions(StringBuilder builder)
        {
            var positions = Account.Orders.Where(o => o.Type == OrderTypes.Position).ToList();
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
    }
}

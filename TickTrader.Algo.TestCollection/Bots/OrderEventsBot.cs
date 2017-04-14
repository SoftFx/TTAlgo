using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Order Events Bot", Version = "2.0", Category = "Test Orders",
        Description = "Subscribes to order events and prints each event info to bot log")]
    public class OrderEventsBot : TradeBotCommon
    {
        protected override void OnStart()
        {
            Account.Orders.Canceled += OrdersOnCanceled;
            Account.Orders.Closed += OrdersOnClosed;
            Account.Orders.Expired += OrdersOnExpired;
            Account.Orders.Filled += OrdersOnFilled;
            Account.Orders.Modified += OrdersOnModified;
            Account.Orders.Opened += OrdersOnOpened;
        }

        protected override void OnStop()
        {
            Account.Orders.Canceled -= OrdersOnCanceled;
            Account.Orders.Closed -= OrdersOnClosed;
            Account.Orders.Expired -= OrdersOnExpired;
            Account.Orders.Filled -= OrdersOnFilled;
            Account.Orders.Modified -= OrdersOnModified;
            Account.Orders.Opened -= OrdersOnOpened;
        }


        private void OrdersOnOpened(OrderOpenedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Opened order #{args.Order.Id}");
            sb.AppendLine(ToObjectPropertiesString("Order", args.Order));
            Print(sb.ToString());
        }

        private void OrdersOnModified(OrderModifiedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Modified order #{args.OldOrder.Id}");
            sb.AppendLine(ToObjectPropertiesString("Old Order", args.OldOrder));
            sb.AppendLine(ToObjectPropertiesString("New Order", args.NewOrder));
            Print(sb.ToString());
        }

        private void OrdersOnFilled(OrderFilledEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Filled order #{args.OldOrder.Id}");
            sb.AppendLine(ToObjectPropertiesString("Old Order", args.OldOrder));
            sb.AppendLine(ToObjectPropertiesString("New Order", args.NewOrder));
            Print(sb.ToString());
        }

        private void OrdersOnExpired(OrderExpiredEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Expired order #{args.Order.Id}");
            sb.AppendLine(ToObjectPropertiesString("Order", args.Order));
            Print(sb.ToString());
        }

        private void OrdersOnClosed(OrderClosedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Closed order #{args.Order.Id}");
            sb.AppendLine(ToObjectPropertiesString("Order", args.Order));
            Print(sb.ToString());
        }

        private void OrdersOnCanceled(OrderCanceledEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Canceled order #{args.Order.Id}");
            sb.AppendLine(ToObjectPropertiesString("Order", args.Order));
            Print(sb.ToString());
        }
    }
}
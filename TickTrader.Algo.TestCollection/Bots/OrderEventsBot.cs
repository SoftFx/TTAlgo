using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Order Events Bot", Version = "2.2", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Subscribes to order events and prints each event info to bot log")]
    public class OrderEventsBot : TradeBotCommon
    {
        protected override void OnStart()
        {
            //Necessary for lazy initialization of the calculator
            var calc = Account.CalculateOrderMargin(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, 0, 1, 0);

            Account.Orders.Canceled += OrdersOnCanceled;
            Account.Orders.Closed += OrdersOnClosed;
            Account.Orders.Expired += OrdersOnExpired;
            Account.Orders.Filled += OrdersOnFilled;
            Account.Orders.Modified += OrdersOnModified;
            Account.Orders.Opened += OrdersOnOpened;
            Account.Orders.Activated += OrderActivated;

            Account.NetPositions.Modified += PositionsModified;
        }

        protected override void OnStop()
        {
            Account.Orders.Canceled -= OrdersOnCanceled;
            Account.Orders.Closed -= OrdersOnClosed;
            Account.Orders.Expired -= OrdersOnExpired;
            Account.Orders.Filled -= OrdersOnFilled;
            Account.Orders.Modified -= OrdersOnModified;
            Account.Orders.Opened -= OrdersOnOpened;
            Account.Orders.Activated -= OrderActivated;

            Account.NetPositions.Modified -= PositionsModified;
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

        private void PositionsModified(NetPositionModifiedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Modified position");
            sb.AppendLine(ToObjectPropertiesString("New Position", args.NewPosition));
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

        private void OrderActivated(OrderActivatedEventArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Activated order #{args.Order.Id} ");
            sb.AppendLine(ToObjectPropertiesString("Order", args.Order));
            Print(sb.ToString());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Event Print Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Bot listens to API events and prints all event data when events are fired")]
    public class EventBot : TradeBot
    {
        private StringBuilder _msgBuilder = new StringBuilder();

        protected override void Init()
        {
            //Necessary for lazy initialization of the calculator
            var calc = Account.CalculateOrderMargin(Symbol.Name, OrderType.Limit, OrderSide.Buy, 0.1, 0, 1, 0);

            Account.BalanceUpdated += () => PrintEventData("Account.BalanceUpdated");
            Account.Reset += () => PrintEventData("Account.Reset");
            Account.Orders.Activated += a => PrintEventData("Account.Orders.Activated", a);
            Account.Orders.Added += a => PrintEventData("Account.Orders.Added", a);
            Account.Orders.Canceled += a => PrintEventData("Account.Orders.Canceled", a);
            Account.Orders.Closed += a => PrintEventData("Account.Orders.Closed", a);
            Account.Orders.Expired += a => PrintEventData("Account.Orders.Expired", a);
            Account.Orders.Filled += a => PrintEventData("Account.Orders.Filled", a);
            Account.Orders.Modified += a => PrintEventData("Account.Orders.Modified", a);
            Account.Orders.Opened += a => PrintEventData("Account.Orders.Opened", a);
            Account.Orders.Removed += a => PrintEventData("Account.Orders.Removed", a);
            Account.Orders.Replaced += a => PrintEventData("Account.Orders.Replaced", a);
            Account.Assets.Modified += a => PrintEventData("Account.Assets.Modified", a);
            Account.NetPositions.Modified += a => PrintEventData("Account.NetPositions.Modified", a);
        }

        private void PrintEventData(string eventName)
        {
            _msgBuilder.Append("EVENT ").Append(eventName);
            DoPrint();
        }

        private void PrintEventData<TData>(string eventName, TData data)
        {
            _msgBuilder.Append("EVENT ").Append(eventName);
            _msgBuilder.PrintPropertiesColumnOfLines(data);
            DoPrint();
        }

        private void DoPrint()
        {            
            var msg = _msgBuilder.ToString();

            Print(msg);
            Status.Write(msg);

            _msgBuilder.Clear();
        }
    }
}

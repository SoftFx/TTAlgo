using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Cancel All Limits/Stops Script")]
    public class CancelAll : TradeBot
    {
        protected override void OnStart()
        {
            if (Account.Type == AccountTypes.Gross)
            {
                var pendings = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();

                foreach (var order in pendings)
                    CancelOrder(order.Id);
            }
            else if (Account.Type == AccountTypes.Cash)
            {
                var positions = this.Account.NetPositions;
                // TO DO : open opposite orders
            }

            Exit();
        }
    }
}

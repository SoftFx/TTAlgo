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
        protected async override void OnStart()
        {
            var pendings = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();

            foreach (var order in pendings)
                await CancelOrderAsync(order.Id);

            Exit();
        }
    }
}

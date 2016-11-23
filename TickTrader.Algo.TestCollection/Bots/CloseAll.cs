using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Close All Positions Script")]
    public class CloseAll : TradeBot
    {
        protected override void OnStart()
        {
            if (Account.Type == AccountTypes.Gross)
            {
                var positions = Account.Orders.Where(o => o.Type == OrderType.Position).ToList();

                foreach (var pos in positions)
                    CloseOrder(pos.Id);
            }
            else if (Account.Type == AccountTypes.Cash)
            {
                var positions = this.Account.NetPositions;
            }

            Exit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [TradeBot(DisplayName = "Close All Positions")]
    public class CloseAllPositiosBot : TradeBot
    {
        protected override void Init()
        {
        }

        protected override void OnStop()
        {
        }

        protected override void OnStart()
        {
            var positions = Account.Orders.Where(o => o.Type == OrderType.Position).ToList();

            foreach (var pos in positions)
            {
                Print("Trying close order #" + pos.Id + " ...");
                CloseOrder(pos.Id);
            }

            Exit();
        }
    }
}

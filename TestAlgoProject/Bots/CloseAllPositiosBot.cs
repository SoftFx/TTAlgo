using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [TradeBot(DisplayName = "Close All Positions Script")]
    public class CloseAllPositiosBot : TradeBot
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

    [TradeBot(DisplayName = "Cancel All Limits/Stops Script")]
    public class CancelAllBot : TradeBot
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

    [TradeBot(DisplayName = "Modify All Limits/Stops prices Script")]
    public class ModifyAllBot : TradeBot
    {
        [Parameter(DisplayName = "Price Modify Delta", DefaultValue = 1)]
        public int PriceDelta { get; set; }

        protected override void OnStart()
        {
            if (Account.Type == AccountTypes.Gross)
            {
                var pendings = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();

                foreach (var order in pendings)
                {
                    var symbol = Symbols[order.Symbol];
                    ModifyOrder(order.Id, order.Price + PriceDelta * symbol.Point);
                }
            }
            else if (Account.Type == AccountTypes.Cash)
            {
                var positions = this.Account.NetPositions;
            }

            Exit();
        }
    }
}

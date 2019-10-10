using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Split View Bot", Version = "1.0", Category = "Test Orders")]
    public class SplitView : TradeBotCommon
    {
        protected override void Init()
        {
            Account.Orders.Splitted += OrderSplitView;
            Account.NetPositions.Splitted += PositionSplitView;
        }

        private void OrderSplitView(OrderSplittedEventArgs args)
        {
            Print($"Order {args.OldOrder} has been splitted to {args.NewOrder}");
        }

        private void PositionSplitView(NetPositionSplittedEventArgs args)
        {
            Print($"Position {args.OldPosition.Id} {args.OldPosition.Symbol} {args.OldPosition.Side} {args.OldPosition.Volume} has been splitted " +
                $"to {args.NewPosition.Id} {args.NewPosition.Symbol} {args.NewPosition.Side} {args.NewPosition.Volume}");
        }
    }
}

using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Split and Dividend View Bot", Version = "1.0", Category = "Test Orders")]
    public class SplitView : TradeBotCommon
    {
        [Parameter(DefaultValue = Mode.All)]
        public Mode Event { get; set; }

        protected override void Init()
        {
            if (Event != Mode.Dividend)
            {
                Account.Orders.Splitted += OrderSplitView;
                Account.NetPositions.Splitted += PositionSplitView;
            }

            if (Event != Mode.Split)
            {
                Account.BalanceDividend += DividendView;
            }
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

        private void DividendView(IBalanceDividendEventArgs args)
        {
            Print($"Dividend {args.Currency} {args.TransactionAmount} balance: {args.Balance}");
        }

        public enum Mode
        {
            All,
            Split,
            Dividend,
        }
    }
}

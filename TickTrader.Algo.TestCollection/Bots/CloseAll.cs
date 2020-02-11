using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Close Positions Script", Version = "1.3", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Closes all positions for gross accounts")]
    public class CloseAll : TradeBot
    {
        [Parameter]
        public string PositionId { get; set; }

        [Parameter(DisplayName = "Target Instance Id", DefaultValue = "")]
        public string TargetInstanceId { get; set; }

        [Parameter(DisplayName = "Close volume", DefaultValue = null)]
        public double? Volume { get; set; }

        protected override void Init()
        {
            var bal = Account.Balance;
        }

        protected async override void OnStart()
        {
            if (Account.Type == AccountTypes.Gross)
            {
                var positions = string.IsNullOrEmpty(TargetInstanceId)
                ? Account.Orders.Where(o => o.Type == OrderType.Position).ToList()
                : Account.Orders.Where(o => o.Type == OrderType.Position && o.InstanceId == TargetInstanceId).ToList();

                if (!string.IsNullOrEmpty(PositionId))
                    positions = positions.Where(o => o.Id == PositionId).ToList();

                foreach (var pos in positions)
                    await CloseOrderAsync(pos.Id, Volume);
            }
            else if (Account.Type == AccountTypes.Net)
            {
                foreach (var position in Account.NetPositions)
                    await OpenOrderAsync(position.Symbol, OrderType.Market, Invert(position.Side), Volume ?? position.Volume, null, 1, null);
            }
            else
                Print("This script works only for Net or Gross accounts!");

            Exit();
        }

        private static OrderSide Invert(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return OrderSide.Sell;
            else
                return OrderSide.Buy;
        }
    }
}

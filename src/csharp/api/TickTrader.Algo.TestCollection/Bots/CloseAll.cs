using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum RequestType { CloseRequest, AsMarket }

    [TradeBot(DisplayName = "[T] Close Positions Script", Version = "1.5", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Closes all positions for margin accounts")]
    public class CloseAll : TradeBot
    {
        [Parameter]
        public string PositionId { get; set; }

        [Parameter]
        public RequestType RequestType { get; set; }

        [Parameter(DisplayName = "Target Instance Id", DefaultValue = "")]
        public string TargetInstanceId { get; set; }

        [Parameter(DisplayName = "Close volume", DefaultValue = null)]
        public double? Volume { get; set; }

        [Parameter(DisplayName = "Close slippage", DefaultValue = null)]
        public double? Slippage { get; set; }

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
                    await CloseOrderAsync(CloseOrderRequest.Template.Create().WithOrderId(pos.Id).WithVolume(Volume).WithSlippage(Slippage ?? pos.Slippage).MakeRequest());
            }
            else if (Account.Type == AccountTypes.Net)
            {
                foreach (var position in Account.NetPositions)
                    if (string.IsNullOrEmpty(PositionId) || position.Id == PositionId)
                    {
                        if (RequestType == RequestType.CloseRequest)
                            await ClosePosition(position);
                        else
                            await ClosePositionAsMarket(position);
                    }
            }
            else
                Print("This script works only for Net or Gross accounts!");

            Exit();
        }

        private async Task ClosePosition(NetPosition position)
        {
            await CloseNetPositionAsync(CloseNetPositionRequest.Template.Create()
                 .WithSymbol(position.Symbol)
                 .WithVolume(Volume ?? position.Volume)
                 .WithSlippage(Slippage)
                 .MakeRequest());
        }

        private async Task ClosePositionAsMarket(NetPosition position)
        {
            await OpenOrderAsync(OpenOrderRequest.Template.Create()
                .WithType(OrderType.Market)
                .WithSymbol(position.Symbol)
                .WithSide(Invert(position.Side))
                .WithVolume(Volume ?? position.Volume)
                .WithSlippage(Slippage)
                .MakeRequest());
        }

        private static OrderSide Invert(OrderSide side) => side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
    }
}

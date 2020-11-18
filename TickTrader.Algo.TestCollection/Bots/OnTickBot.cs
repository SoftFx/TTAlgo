using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Market Order Per Tick Bot", Version = "1.0", Category = "Test Orders",
        Description = "Open market order on every tick of current chart symbol with specified volume and side")]
    public class OnTickBot : TradeBot
    {
        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        protected override void Init()
        {
            Symbol.Subscribe();
        }

        protected override void OnModelTick()
        {
            OpenMarketOrder(Side, Volume);
        }
    }
}

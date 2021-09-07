using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots.Trade
{

    [TradeBot(DisplayName = "[T] Fast Trading bot", Version = "1.0")]
    class FastTradingBot : TradeBot
    {
        [Parameter(DefaultValue = 0.01)]
        public double MinVolume { get; set; }

        [Parameter(DefaultValue = 1.0)]
        public double MaxVolume { get; set; }

        [Parameter(DefaultValue = 2.0)]
        public double TotalVolume { get; set; }

        [Parameter(DefaultValue = OrderSide.Sell)]
        public OrderSide Side { get; set; }

        [Parameter]
        public double Price { get; set; }


        protected async override void Init()
        {
            Random random = new Random();
            var count = 0;
            var openedVolume = 0.0;

            while (TotalVolume.Gte(Symbol.MinTradeVolume))
            {
                var current = Math.Min(random.NextDouble() * (MaxVolume - MinVolume) + MinVolume, TotalVolume);
                var template = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, Side, OrderType.Limit, current, Price, null);

                TotalVolume -= current;
                count++;

                var response = await OpenOrderAsync(template.MakeRequest());

                openedVolume += response.ResultingOrder.RemainingVolume;
            }

            Status.WriteLine($"Total count orders: {count}, volume = {openedVolume}");
            Exit();
        }
    }
}

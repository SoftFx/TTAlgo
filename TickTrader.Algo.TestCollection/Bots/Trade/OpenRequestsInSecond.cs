using System.Diagnostics;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots.Trade
{
    [TradeBot(DisplayName = "[T] Open Requests In Second", Version = "1.0")]
    class OpenRequestsInSecond : TradeBot
    {
        [Parameter(DefaultValue = 100)]
        public int RequestCount { get; set; }

        protected override void OnStart()
        {
            var st = Stopwatch.StartNew();

            for (int i = 0; i < RequestCount; ++i)
                OpenOrderAsync(OpenOrderRequest.Template.Create().WithParams("EURUSD", OrderSide.Buy, OrderType.Limit, 1.0, 0.1, null).MakeRequest());

            st.Stop();

            Status.WriteLine($"Total time: {st.ElapsedTicks / 1e4} ms");
        }
    }
}

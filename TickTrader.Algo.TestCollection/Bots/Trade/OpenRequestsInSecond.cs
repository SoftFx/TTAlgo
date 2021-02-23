using System;
using System.Diagnostics;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots.Trade
{
    [TradeBot(DisplayName = "[T] Open Requests In Second", Version = "1.0")]
    class OpenRequestsInSecond : TradeBot
    {
        private Stopwatch _stopwatch;
        private bool _run = false;

        [Parameter(DefaultValue = 100)]
        public int RequestCount { get; set; }

        protected override void Init()
        {
            _stopwatch = new Stopwatch();
            _run = true;
        }

        protected async override void OnStart()
        {
            while (_run)
            {
                await Delay(1000 - DateTime.Now.Millisecond);

                var side = DateTime.Now.Second % 2 == 0 ? OrderSide.Buy : OrderSide.Sell;

                _stopwatch.Restart();

                for (int i = 0; i < RequestCount; ++i)
                    OpenOrderAsync(OpenOrderRequest.Template.Create().WithParams(Symbol.Name, side, OrderType.Market, 1.0, 0.1, null).MakeRequest());

                _stopwatch.Stop();

                Status.WriteLine($"Total time: {_stopwatch.ElapsedTicks / 1e4} ms");
            }
        }

        protected override void OnStop()
        {
            _run = false;
        }
    }
}

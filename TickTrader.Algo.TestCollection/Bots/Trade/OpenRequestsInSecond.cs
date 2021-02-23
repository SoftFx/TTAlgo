﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        [Parameter(DisplayName = "Open Mode")]
        public OpenMode Mode { get; set; }

        protected override void Init()
        {
            _stopwatch = new Stopwatch();
            _run = true;
        }

        protected async override void OnStart()
        {
            int countInGroup = RequestCount / 10;
            var side = OrderSide.Buy;
            var group = 0;
            var systemTime = 0;
            var countRequestPerSec = 0;

            while (_run)
            {
                if (Mode == OpenMode.Exponential)
                {
                    await Delay(1000 - DateTime.UtcNow.Millisecond);

                    side = DateTime.Now.Second % 2 == 0 ? OrderSide.Buy : OrderSide.Sell;

                    _stopwatch.Restart();

                    for (int i = 0; i < RequestCount; ++i)
                        SendRequest(side);

                    _stopwatch.Stop();

                    Status.WriteLine($"Total time: {_stopwatch.ElapsedTicks / 1e4} ms");
                }
                else
                {
                    await Task.Delay(Math.Max(100 - systemTime, 0));

                    _stopwatch.Restart();

                    if (++group == 10)
                    {
                        Status.WriteLine($"Operations per sec: {countRequestPerSec}");
                        side = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                        group = 0;
                        countRequestPerSec = 0;
                    }

                    for (int i = 0; i < countInGroup; ++i, ++countRequestPerSec)
                        SendRequest(side);

                    _stopwatch.Stop();
                    systemTime = (int)_stopwatch.ElapsedMilliseconds;
                }
            }
        }

        protected override void OnStop()
        {
            _run = false;
        }

        private void SendRequest(OrderSide side) => OpenOrderAsync(OpenOrderRequest.Template.Create().WithParams(Symbol.Name, side, OrderType.Market, 1.0, 0.1, null).MakeRequest());
    }

    public enum OpenMode { Uniform, Exponential }
}

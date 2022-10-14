using System;
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

        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }


        protected override void Init()
        {
            _stopwatch = new Stopwatch();
            _run = true;
        }

        protected async override void OnStart()
        {
            int countInGroup = Math.Max(RequestCount / 10, 1);
            int updateFreq = Math.Min(RequestCount, 10);
            int timePeriod = 1000 / updateFreq;

            var side = OrderSide.Buy;
            var group = 0;
            var systemTime = 0;
            var countRequestPerSec = 0;
            var totalWorkingTime = 0L;

            var tasks = new Task[countInGroup];

            while (_run)
            {
                if (Mode == OpenMode.Exponential)
                {
                    await Delay(1000 - UtcNow.Millisecond);

                    side = UtcNow.Second % 2 == 0 ? OrderSide.Buy : OrderSide.Sell;

                    _stopwatch.Restart();

                    for (int i = 0; i < RequestCount; ++i)
                        SendRequest(side);

                    _stopwatch.Stop();

                    Status.WriteLine($"Total time: {_stopwatch.ElapsedTicks / 1e4} ms");
                }
                else
                {
                    await Task.Delay(Math.Max(timePeriod - systemTime, 0));

                    if (++group == updateFreq)
                    {
                        Status.WriteLine($"Operations per sec: {countRequestPerSec}");
                        //Status.WriteLine($"Total request time: {totalWorkingTime} ms");

                        side = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                        group = 0;
                        countRequestPerSec = 0;
                        totalWorkingTime = 0L;
                    }

                    _stopwatch.Restart();

                    for (int i = 0; i < countInGroup; ++i, ++countRequestPerSec)
                        tasks[i] = SendRequest(side);

                    //Task.WaitAll(tasks);

                    _stopwatch.Stop();

                    totalWorkingTime += _stopwatch.ElapsedMilliseconds;
                    systemTime = (int)_stopwatch.ElapsedMilliseconds;
                }
            }
        }

        protected override void OnStop()
        {
            _run = false;
        }

        private Task SendRequest(OrderSide side) => OpenOrderAsync(OpenOrderRequest.Template.Create().WithParams(Symbol.Name, side, OrderType.Market, Volume, 0.1, null).MakeRequest());
    }

    public enum OpenMode { Uniform, Exponential }
}

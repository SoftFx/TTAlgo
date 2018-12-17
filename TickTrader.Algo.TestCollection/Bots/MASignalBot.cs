using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Moving Average Signal Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Prints MovingAverage signal to status")]
    public class MASignalBot : TradeBot
    {
        private IMovingAverage _movingAverage;


        [Parameter(DefaultValue = 12, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 6, DisplayName = "Shift")]
        public int Shift { get; set; }


        protected override void Init()
        {
            _movingAverage = Indicators.MovingAverage(Bars.Close, Period, Shift);
        }

        protected override void OnStart()
        {
            Loop();
        }


        private async void Loop()
        {
            while(!IsStopped)
            {
                var close = Bars[1].Close;
                var open = Bars[1].Open;
                var ma = _movingAverage.Average[0];
                Status.WriteLine($"Close: {close}");
                Status.WriteLine($"Open: {open}");
                Status.WriteLine($"MA: {ma}");
                if (!double.IsNaN(ma))
                {
                    if (open > ma && close < ma)
                        Status.WriteLine("Close Buy");
                    if (open < ma && close > ma)
                        Status.WriteLine("Close Sell");
                }
                Status.Flush();
                await Task.Delay(1000);
            }
        }
    }
}

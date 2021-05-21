using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Volumes.OnBalanceVolume
{
    [Indicator(Category = "Volumes", DisplayName = "On Balance Volume", Version = "1.0")]
    public class OnBalanceVolume : Indicator, IOnBalanceVolume
    {
        [Parameter(DefaultValue = AppliedPrice.Close, DisplayName = "Apply To")]
        public AppliedPrice TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        public DataSeries Price { get; private set; }

        [Output(DisplayName = "OBV", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Obv { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public OnBalanceVolume() { }

        public OnBalanceVolume(BarSeries bars, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            Bars = bars;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            Price = AppliedPriceHelper.GetDataSeries(Bars, TargetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Price.Count > 1)
            {
                //if (Math.Abs(Price[pos] - Price[pos + 1]) < 1e-20)
                if (Math.Abs(Price[pos] - Price[pos+1]) < 1e-20)
                {
                    Obv[pos] = Obv[pos + 1];
                }
                else if (Price[pos] < Price[pos + 1])
                {
                    Obv[pos] = Obv[pos + 1] - Bars.Volume[pos];
                }
                else// if (Price[pos] < Price[pos + 1])
                {
                    Obv[pos] = Obv[pos + 1] + Bars.Volume[pos];
                }
            }
            else
            {
                Obv[pos] = Bars.Volume[pos];
            }
        }
    }
}

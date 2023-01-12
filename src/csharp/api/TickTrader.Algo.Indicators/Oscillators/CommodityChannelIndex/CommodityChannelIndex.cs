using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Oscillators.CommodityChannelIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Commodity Channel Index", Version = "1.1")]
    public class CommodityChannelIndex : Indicator, ICommodityChannelIndex
    {
        private IMovingAverage _sma;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "CCI", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Cci { get; set; }

        public int LastPositionChanged { get { return _sma.LastPositionChanged; } }

        public CommodityChannelIndex() { }

        public CommodityChannelIndex(DataSeries price, int period)
        {
            Price = price;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _sma = Indicators.MovingAverage(Price, Period, 0);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var average = _sma.Average[pos];
            var meanDeviation = 0.0;
            if (double.IsNaN(average))
            {
                meanDeviation = double.NaN;
            }
            else
            {
                for (var i = pos; i < pos + Period; i++)
                {
                    meanDeviation += Math.Abs(average - Price[i]);
                }
                meanDeviation /= Period;
            }
            Cci[pos] = (Price[pos] - average)/(0.015*meanDeviation);
        }
    }
}

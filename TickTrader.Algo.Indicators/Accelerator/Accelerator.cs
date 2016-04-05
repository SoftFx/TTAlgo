using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;


namespace TickTrader.Algo.Indicators.Accelerator
{
    [Indicator]
    public class Accelerator : Indicator
    {
        [Parameter(DefaultValue = 5)]
        public int PeriodFast { get; set; }

        [Parameter(DefaultValue = 34)]
        public int PeriodSlow { get; set; }

        [Parameter(DefaultValue = 38)]
        public int DataLimit { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output]
        public DataSeries ExtACBuffer { get; set; }

        [Output]
        public DataSeries ExtUpBuffer { get; set; }

        [Output]
        public DataSeries ExtDnBuffer { get; set; }

        protected override void Calculate()
        {

            double Signal = 0.0;
            double prevSignal = 0.0;
      

            if (Bars.Count >= 6 + Math.Max(PeriodFast, PeriodSlow))
            {
                double macd = GetMACD(0);
                double macdPrev = GetMACD(1);

                for (int i = 0; i < 5; i++)
                {
                    Signal += GetMACD(i);
                }
                Signal /= 5;

                for (int i = 1; i < 6; i++)
                {
                    prevSignal += GetMACD(i);
                }
                prevSignal /= 5;

                bool up = true;
                double current = macd - Signal;
                double prev = macdPrev - prevSignal;
                up = current > prev;
                if (!up)
                {
                    ExtUpBuffer[0] = 0.0;
                    ExtDnBuffer[0] = current;
                }
                else
                {
                    ExtUpBuffer[0] = current;
                    ExtDnBuffer[0] = 0.0;
                }
                ExtACBuffer[0] = current;
            }

            }
        private double GetMACD(int pos)
        {
            double fastRes = MovingAverages.SimpleMA(pos, PeriodFast,
                Bars.Take(pos + PeriodFast).Select(b => (b.High + b.Low)/2).ToList());
            double slowRes = MovingAverages.SimpleMA(pos, PeriodSlow,
                Bars.Take(pos + PeriodSlow).Select(b => (b.High + b.Low) / 2).ToList());
            return fastRes - slowRes;
        }

    }


}

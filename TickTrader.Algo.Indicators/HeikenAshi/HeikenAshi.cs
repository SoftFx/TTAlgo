using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.HeikenAshi
{

    [Indicator]
    public class HeikenAshi : Indicator
    {
        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtLowHighBuffer { get; set; }
        [Output]
        public DataSeries ExtHighLowBuffer { get; set; }
        [Output]
        public DataSeries ExtOpenBuffer { get; set; }
        [Output]
        public DataSeries ExtCloseBuffer { get; set; }

        protected override void Calculate()
        {
            ExtLowHighBuffer[0] = Double.NaN;
            ExtHighLowBuffer[0] = Double.NaN;
            ExtCloseBuffer[0] = Double.NaN;
            ExtOpenBuffer[0] = Double.NaN;
            if (Bars.Count == 1)
            {
                if (Bars[0].Open < Bars[0].Close)
                {
                    ExtLowHighBuffer[0] = Bars[0].Low;
                    ExtHighLowBuffer[0] = Bars[0].High;
                }
                else
                {
                    ExtLowHighBuffer[0] = Bars[0].High;
                    ExtHighLowBuffer[0] = Bars[0].Low;
                }
                ExtOpenBuffer[0] = Bars[0].Open;
                ExtCloseBuffer[0] = Bars[0].Close;
            }

            if (Bars.Count > 1)
            {
                double haOpen = (ExtOpenBuffer[1] + ExtCloseBuffer[1]) / 2;
                double haClose = (Bars[0].Open + Bars[0].High + Bars[0].Low + Bars[0].Close) / 4;
                double haHigh = Math.Max(Bars[0].High, Math.Max(haOpen, haClose));
                double haLow = Math.Min(Bars[0].Low, Math.Min(haOpen, haClose));
                if (haOpen < haClose)
                {
                    ExtLowHighBuffer[0] = haLow;
                    ExtHighLowBuffer[0] = haHigh;
                }
                else
                {
                    ExtLowHighBuffer[0] = haHigh;
                    ExtHighLowBuffer[0] = haLow;
                }
                ExtOpenBuffer[0] = haOpen;
                ExtCloseBuffer[0] = haClose;
            }



        }
    }
}

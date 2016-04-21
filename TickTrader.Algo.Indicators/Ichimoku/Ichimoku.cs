using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Ichimoku
{

    [Indicator(IsOverlay = true)]
    public class Ichimoku : Indicator
    {

        [Parameter(DefaultValue = 9)]
        public int InpTenkan { get; set; }
        [Parameter(DefaultValue = 26)]
        public int InpKijun { get; set; }
        [Parameter(DefaultValue = 52)]
        public int InpSenkou { get; set; }


        [Input]
        public BarSeries Bars { get; set; }




        [Output]
        public DataSeries ExtTenkanBuffer { get; set; }
        [Output]
        public DataSeries ExtKijunBuffer { get; set; }
        [Output]
        public DataSeries ExtSpanA_Buffer { get; set; }
        [Output]
        public DataSeries ExtSpanB_Buffer { get; set; }
        [Output]
        public DataSeries ExtChikouBuffer { get; set; }
        [Output]
        public DataSeries ExtSpanA2_Buffer { get; set; }
        [Output]
        public DataSeries ExtSpanB2_Buffer { get; set; }

        protected override void Calculate()
        {

            double high_value, low_value;

            //--- Tenkan Sen
            if (Bars.Count >= InpTenkan)
            {
                high_value = Bars[0].High;
                low_value = Bars[0].Low;
                for (int i = 1; i < InpTenkan; i++)
                {
                    if (high_value < Bars[i].High)
                        high_value = Bars[i].High;
                    if (low_value > Bars[i].Low)
                        low_value = Bars[i].Low;
                }
                ExtTenkanBuffer[0] = (high_value + low_value) / 2;
            }


            //--- Kijun Sen
            if (Bars.Count >= InpKijun)
            {
                high_value = Bars[0].High;
                low_value = Bars[0].Low;
                for (int i = 1; i < InpKijun; i++)
                {
                    if (high_value < Bars[i].High)
                        high_value = Bars[i].High;
                    if (low_value > Bars[i].Low)
                        low_value = Bars[i].Low;
                }
                ExtKijunBuffer[0] = (high_value + low_value) / 2;
            }

            //--- Senkou Span A
            if (Bars.Count > InpKijun)
            {
                ExtSpanA_Buffer[0] = (ExtKijunBuffer[InpKijun] + ExtTenkanBuffer[InpKijun]) / 2;
                ExtSpanA2_Buffer[0] = ExtSpanA_Buffer[0];
            }

            //--- Senkou Span B
            if (Bars.Count >= InpSenkou + InpKijun)
            {
                high_value = Bars[InpKijun].High;
                low_value = Bars[InpKijun].Low;
                for (int i = 1; i < InpSenkou; i++)
                {
                    if (high_value < Bars[InpKijun + i].High)
                        high_value = Bars[InpKijun + i].High;
                    if (low_value > Bars[InpKijun + i].Low)
                        low_value = Bars[InpKijun + i].Low;
                }
                ExtSpanB_Buffer[0] = (high_value + low_value) / 2;
                ExtSpanB2_Buffer[0] = ExtSpanB_Buffer[0];
            }

            //--- Chikou Span
            if (Bars.Count > InpKijun)
            {
                ExtChikouBuffer[InpKijun] = Bars[0].Close;
            }

        }
    }
}

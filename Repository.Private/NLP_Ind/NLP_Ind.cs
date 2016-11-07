using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace NLP_Ind
{
    [Indicator(IsOverlay = true, Category = "Custom", DisplayName = "NLP")]
    public class NLP_Ind : Indicator
    {
        [Parameter(DefaultValue = 100, DisplayName = "Sensitivity")]
        public int Sensitivity { get; set; }

        [Parameter(DefaultValue = 10, DisplayName = "Spread")]
        public int Spread { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        //[Output(DisplayName = "MaxBid", DefaultColor = Colors.Red, PlotType = PlotType.DiscontinuousLine)]
        //public DataSeries MaxBid { get; set; }

        //[Output(DisplayName = "MinAsk", DefaultColor = Colors.Gold, PlotType = PlotType.DiscontinuousLine)]
        //public DataSeries MinAsk { get; set; }

        [Output(DisplayName = "Q95", DefaultColor = Colors.Gold, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries Q95 { get; set; }

        [Output(DisplayName = "Q5", DefaultColor = Colors.Red, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries Q5 { get; set; }

        [Output(DisplayName = "Q50", DefaultColor = Colors.Green, PlotType = PlotType.DiscontinuousLine)]
        public DataSeries Q50 { get; set; }

        MarketState mState;
        RemoteCalculation remoteCalc;
        protected override void Init()
        {
            mState = new MarketState(Sensitivity);
            remoteCalc = new RemoteCalculation("123");
            //remoteCalc.Init(this.Symbol.Name, Sensitivity, new double[] { 0.05, 0.5, 0.95 });
        }
        protected override void Calculate()
        {
            if (Bars.Count <= 1)
                return;

            int maxBid = (int)Math.Ceiling(Bars[1].High / Symbol.Point);
            int minBid = (int)Math.Ceiling(Bars[1].Low / Symbol.Point);
            mState.ProcessQuote(maxBid, maxBid + Spread, Bars[1].CloseTime);
            mState.ProcessQuote(minBid, minBid + Spread, Bars[1].CloseTime);

            double[] res = remoteCalc.RequestQuantiles(mState.PrevAmplitude, mState.PrevDuration, mState.CurrentAmplitude, mState.CurrentDuration, mState.TrendUP);
            if (res == null)
                return;

            if (mState.TrendUP)
            {
                Q5[1] = (mState.MinAsk + res[0])*this.Symbol.Point;
                Q50[1] = (mState.MinAsk + res[1]) * this.Symbol.Point;
                Q95[1] = (mState.MinAsk + res[2]) * this.Symbol.Point;
            }
            else
            {
                Q5[1] = (mState.MaxBid  - res[0]) * this.Symbol.Point;
                Q50[1] = (mState.MaxBid - res[1]) * this.Symbol.Point;
                Q95[1] = (mState.MaxBid - res[2]) * this.Symbol.Point;
            }

            //if ( mState.TrendUP)
            //{
            //    MaxBid[1] = mState.MaxBid * Symbol.Point;
            //    MinAsk[1] = double.NaN;
            //}
            //else
            //{
            //    MinAsk[1] = mState.MinAsk * Symbol.Point;
            //    MaxBid[1] = double.NaN;
            //}
        }

    }
}
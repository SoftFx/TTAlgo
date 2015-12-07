using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Alligator
{

    [Indicator]
    public class Alligator : Indicator
    {
        [Parameter(DefaultValue = 13)]
        public int InpJawsPeriod { get; set; }

        [Parameter(DefaultValue = 8)]
        public int InpJawsShift { get; set; }

        [Parameter(DefaultValue = 8)]
        public int InpTeethPeriod { get; set; }

        [Parameter(DefaultValue = 5)]
        public int InpTeethShift { get; set; }

        [Parameter(DefaultValue = 5)]
        public int InpLipsPeriod { get; set; }
        [Parameter(DefaultValue = 3)]
        public int InpLipsShift { get; set; }
        

        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtBlueBuffer { get; set; }
        
        [Output]
        public DataSeries ExtRedBuffer { get; set; }

        [Output]
        public DataSeries ExtLimeBuffer { get; set; }
        
        


        protected override void Calculate()
        {
            ExtBlueBuffer[0] = MovingAverages.SmoothedMAinPlace(InpJawsShift, InpJawsPeriod,
                (Bars.Select(b => (b.High + b.Low)/2)).ToList());
            /*ExtBlueBuffer[0] = MovingAverages.SmoothedMA(InpJawsShift, InpJawsPeriod, ExtBlueBuffer[1],
                (Bars.Select(b => (b.High + b.Low)/2)).ToList());*/
            ExtRedBuffer[0] = Indicators.Functions.MovingAverages.SmoothedMAinPlace(InpTeethShift,
                InpTeethPeriod, (Bars.Select(b => (b.High + b.Low) / 2)).ToList());
            /*ExtRedBuffer[0] = Indicators.Functions.MovingAverages.SmoothedMA(InpTeethShift,
                InpTeethPeriod, ExtRedBuffer[1], (Bars.Select(b => (b.High + b.Low) / 2)).ToList());*/
            ExtLimeBuffer[0] = Indicators.Functions.MovingAverages.SmoothedMAinPlace(InpLipsShift,
                InpLipsPeriod, (Bars.Select(b => (b.High + b.Low) / 2)).ToList());
            /*ExtLimeBuffer[0] = Indicators.Functions.MovingAverages.SmoothedMA(InpLipsShift,
                InpLipsPeriod, ExtLimeBuffer[1], (Bars.Select(b => (b.High + b.Low) / 2)).ToList());*/
        }

        
        
    }
}

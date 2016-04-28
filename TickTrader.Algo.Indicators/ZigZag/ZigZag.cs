using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.ZigZag
{

    [Indicator(IsOverlay = true, DisplayName = "Zigzag")]
    public class ZigZag : Indicator
    {
        [Parameter(DefaultValue = 12, DisplayName = "Depth")]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Deviation")]
        public int Deviation { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "BackStep")]
        public int Backstep { get; set; }

        [Parameter(DisplayName = "Point Size", DefaultValue = 10e-5)]
        public double PointSize { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Zigzag", DefaultColor = Colors.Red)]
        public DataSeries Zigzag { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ZigZag() { }

        public ZigZag(BarSeries bars, int depth, int deviation, int backstep, double pointSize)
        {
            Bars = bars;
            Depth = depth;
            Deviation = deviation;
            Backstep = backstep;
            PointSize = pointSize;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        private double extremum;
        double lastZZlow = 0.0, lastZZhigh = 0.0, lasthigh = 0.0, lastlow = 0.0;
        private List<double> ExtLowBuffer = new List<double>();
        private List<double> ExtHighBuffer = new List<double>();
        int whatlookfor = 0;
        int lasthighpos = 0, lastlowpos = 0;
        protected override void Calculate()
        {

            ExtHighBuffer.Add(0.0);
            ExtLowBuffer.Add(0.0);
            //if (Bars.Count == 1430)
            //{
            //    string[] a = Bars.Select(b => (b.High * 100 - 110).ToString()).ToArray();
            //    a = a.Reverse().ToArray();
            //    System.IO.File.WriteAllLines(@"D:\out1.txt", a);
            //}
            if (Bars.Count >= Math.Max(Depth, Backstep) + 1)
            {
                //--- find lowest low in depth of bars
                extremum = PeriodHelper.FindMin(Bars.Low, Depth);
                //--- this lowest has been found previously
                if (extremum == lastlow)
                    extremum = 0.0;
                else
                {
                    //--- new last low
                    lastlow = extremum;

                    //--- discard extremum if current low is too high
                    if (Bars[0].Low - extremum > Deviation * PointSize)
                        extremum = 0.0;
                    else
                    {
                        //--- clear previous extremums in backstep bars
                        for (int back = 1; back <= Backstep; back++)
                        {
                            int pos = back;
                            if (ExtLowBuffer[Bars.Count - 1 - pos] != 0 && ExtLowBuffer[Bars.Count - 1 - pos] > extremum)
                                ExtLowBuffer[Bars.Count - 1 - pos] = 0.0;
                        }
                    }
                }
                //--- found extremum is current low
                if (Bars[0].Low == extremum)
                    ExtLowBuffer[Bars.Count - 1] = extremum;
                else
                    ExtLowBuffer[Bars.Count - 1] = 0.0;
                //--- find highest high in depth of bars
                extremum = PeriodHelper.FindMax(Bars.High, Depth);
                //--- this highest has been found previously
                if (extremum == lasthigh)
                    extremum = 0.0;
                else
                {
                    //--- new last high
                    lasthigh = extremum;
                    //--- discard extremum if current high is too low
                    if (extremum - Bars[0].High > Deviation * PointSize)
                        extremum = 0.0;
                    else
                    {
                        //--- clear previous extremums in backstep bars
                        for (int back = 1; back <= Backstep; back++)
                        {
                            int pos = back;
                            if (ExtHighBuffer[Bars.Count - 1 - pos] != 0 &&
                                ExtHighBuffer[Bars.Count - 1 - pos] < extremum)
                                ExtHighBuffer[Bars.Count - 1 - pos] = 0.0;
                        }
                    }
                }
                //--- found extremum is current high
                if (Bars[0].High == extremum)
                    ExtHighBuffer[Bars.Count - 1] = extremum;
                else
                    ExtHighBuffer[Bars.Count - 1] = 0.0;


                if (whatlookfor == 0)
                {
                    lastZZlow = 0.0;
                    lastZZhigh = 0.0;
                }
                switch (whatlookfor)
                {
                    case 0: // look for peak or lawn 
                        if (lastZZlow == 0.0 && lastZZhigh == 0.0)
                        {
                            if (ExtHighBuffer[Bars.Count - 1 - Backstep] != 0.0)
                            {
                                lastZZhigh = Bars[Backstep].High;
                                lasthighpos = Bars.Count;
                                whatlookfor = -1;
                                Zigzag[Backstep] = lastZZhigh;
                            }
                            if (ExtLowBuffer[Bars.Count - 1 - Backstep] != 0.0)
                            {
                                lastZZlow = Bars[Backstep].Low;
                                lastlowpos = Bars.Count;
                                whatlookfor = 1;
                                Zigzag[Backstep] = lastZZlow;
                            }
                        }
                        break;
                    case 1: // look for peak
                        if (ExtLowBuffer[Bars.Count - 1 - Backstep] != 0.0 && ExtLowBuffer[Bars.Count - 1 - Backstep] < lastZZlow &&
                            ExtHighBuffer[Bars.Count - 1 - Backstep] == 0.0)
                        {
                            Zigzag[Bars.Count - lastlowpos + Backstep] = Double.NaN;
                            lastlowpos = Bars.Count;
                            lastZZlow = ExtLowBuffer[Bars.Count - 1 - Backstep];
                            Zigzag[Backstep] = lastZZlow;
                        }
                        if (ExtHighBuffer[Bars.Count - 1 - Backstep] != 0.0 && ExtLowBuffer[Bars.Count - 1 - Backstep] == 0.0)
                        {
                            lastZZhigh = ExtHighBuffer[Bars.Count - 1 - Backstep];
                            lasthighpos = Bars.Count;
                            Zigzag[Backstep] = lastZZhigh;
                            whatlookfor = -1;
                        }
                        break;
                    case -1: // look for lawn
                        if (ExtHighBuffer[Bars.Count - 1 - Backstep] != 0.0 && ExtHighBuffer[Bars.Count - 1 - Backstep] > lastZZhigh &&
                            ExtLowBuffer[Bars.Count - 1 - Backstep] == 0.0)
                        {
                            Zigzag[Bars.Count - lasthighpos + Backstep] = Double.NaN;
                            lasthighpos = Bars.Count;
                            lastZZhigh = ExtHighBuffer[Bars.Count - 1 - Backstep];
                            Zigzag[Backstep] = lastZZhigh;
                        }
                        if (ExtLowBuffer[Bars.Count - 1 - Backstep] != 0.0 && ExtHighBuffer[Bars.Count - 1 - Backstep] == 0.0)
                        {
                            lastZZlow = ExtLowBuffer[Bars.Count - 1 - Backstep];
                            lastlowpos = Bars.Count;
                            Zigzag[Backstep] = lastZZlow;
                            whatlookfor = 1;
                        }
                        break;
                }
            }

        }
    }
}

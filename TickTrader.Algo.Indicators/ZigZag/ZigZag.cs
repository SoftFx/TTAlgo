using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.ZigZag
{

    [Indicator]
    public class ZigZag : Indicator
    {


        [Parameter(DefaultValue = 12)]
        public int InpDepth { get; set; }

        [Parameter(DefaultValue = 5)]
        public int InpDeviation { get; set; }

        [Parameter(DefaultValue = 3)]
        public int InpBackstep { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }



        [Output]
        public DataSeries ExtZigzagBuffer { get; set; }

        private double extremum;
        private double Point = 0.00001;
        double lastZZlow = 0.0, lastZZhigh = 0.0, lasthigh = 0.0, lastlow = 0.0;
        private List<double> ExtLowBuffer = new List<double>();
        private List<double> ExtHighBuffer = new List<double>();
        int whatlookfor = 0;
        int lasthighpos = 0, lastlowpos = 0;
        protected override void Calculate()
        {
            ExtZigzagBuffer[0] = 0.0;
            ExtHighBuffer.Add(0.0);
            ExtLowBuffer.Add(0.0);
            if (Bars.Count == 1430)
            {
                string[] a = Bars.Select(b => (b.High * 100 - 110).ToString()).ToArray();
                a = a.Reverse().ToArray();
                System.IO.File.WriteAllLines(@"D:\out1.txt", a);
            }
            if (Bars.Count >= Math.Max(InpDepth, InpBackstep) + 1)
            {
                //--- find lowest low in depth of bars
                extremum = Bars.Take(Convert.ToInt32(InpDepth)).Select(b => b.Low).ToList().Min();
                //--- this lowest has been found previously
                if (extremum == lastlow)
                    extremum = 0.0;
                else
                {
                    //--- new last low
                    lastlow = extremum;

                    //--- discard extremum if current low is too high
                    if (Bars[0].Low - extremum > InpDeviation * Point)
                        extremum = 0.0;
                    else
                    {
                        //--- clear previous extremums in backstep bars
                        for (int back = 1; back <= InpBackstep; back++)
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
                extremum = Bars.Take(Convert.ToInt32(InpDepth)).Select(b => b.High).ToList().Max();
                //--- this highest has been found previously
                if (extremum == lasthigh)
                    extremum = 0.0;
                else
                {
                    //--- new last high
                    lasthigh = extremum;
                    //--- discard extremum if current high is too low
                    if (extremum - Bars[0].High > InpDeviation * Point)
                        extremum = 0.0;
                    else
                    {
                        //--- clear previous extremums in backstep bars
                        for (int back = 1; back <= InpBackstep; back++)
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
                if (Bars.Count >= 1353)
                {
                    int a = 1;
                }
                switch (whatlookfor)
                {
                    case 0: // look for peak or lawn 
                        if (lastZZlow == 0.0 && lastZZhigh == 0.0)
                        {
                            if (ExtHighBuffer[Bars.Count - 1 - InpBackstep] != 0.0)
                            {
                                lastZZhigh = Bars[InpBackstep].High;
                                lasthighpos = Bars.Count;
                                whatlookfor = -1;
                                ExtZigzagBuffer[InpBackstep] = lastZZhigh;
                            }
                            if (ExtLowBuffer[Bars.Count - 1 - InpBackstep] != 0.0)
                            {
                                lastZZlow = Bars[InpBackstep].Low;
                                lastlowpos = Bars.Count;
                                whatlookfor = 1;
                                ExtZigzagBuffer[InpBackstep] = lastZZlow;
                            }
                        }
                        break;
                    case 1: // look for peak
                        if (ExtLowBuffer[Bars.Count - 1 - InpBackstep] != 0.0 && ExtLowBuffer[Bars.Count - 1 - InpBackstep] < lastZZlow &&
                            ExtHighBuffer[Bars.Count - 1 - InpBackstep] == 0.0)
                        {
                            ExtZigzagBuffer[Bars.Count - lastlowpos + InpBackstep] = 0.0;
                            lastlowpos = Bars.Count;
                            lastZZlow = ExtLowBuffer[Bars.Count - 1 - InpBackstep];
                            ExtZigzagBuffer[InpBackstep] = lastZZlow;
                        }
                        if (ExtHighBuffer[Bars.Count - 1 - InpBackstep] != 0.0 && ExtLowBuffer[Bars.Count - 1 - InpBackstep] == 0.0)
                        {
                            lastZZhigh = ExtHighBuffer[Bars.Count - 1 - InpBackstep];
                            lasthighpos = Bars.Count;
                            ExtZigzagBuffer[InpBackstep] = lastZZhigh;
                            whatlookfor = -1;
                        }
                        break;
                    case -1: // look for lawn
                        if (ExtHighBuffer[Bars.Count - 1 - InpBackstep] != 0.0 && ExtHighBuffer[Bars.Count - 1 - InpBackstep] > lastZZhigh &&
                            ExtLowBuffer[Bars.Count - 1 - InpBackstep] == 0.0)
                        {
                            ExtZigzagBuffer[Bars.Count - lasthighpos + InpBackstep] = 0.0;
                            lasthighpos = Bars.Count;
                            lastZZhigh = ExtHighBuffer[Bars.Count - 1 - InpBackstep];
                            ExtZigzagBuffer[InpBackstep] = lastZZhigh;
                        }
                        if (ExtLowBuffer[Bars.Count - 1 - InpBackstep] != 0.0 && ExtHighBuffer[Bars.Count - 1 - InpBackstep] == 0.0)
                        {
                            lastZZlow = ExtLowBuffer[Bars.Count - 1 - InpBackstep];
                            lastlowpos = Bars.Count;
                            ExtZigzagBuffer[InpBackstep] = lastZZlow;
                            whatlookfor = 1;
                        }
                        break;
                }
            }

        }

    }
}

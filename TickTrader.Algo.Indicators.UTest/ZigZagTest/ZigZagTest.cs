using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.ZigZagTest
{
    [TestClass]
    public class ZigZagTest
    {

        private StreamReader file;
        private List<double> metaResZZ;
        private List<double> testResZZ;


        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResZZ = new List<double>();
            testResZZ = new List<double>();



            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtZigzagBuffer", testResZZ);


            builder = new IndicatorBuilder<Bar>(typeof(ZigZag.ZigZag), reader, writer);
            builder.SetParameter("InpDepth", 12);
            builder.SetParameter("InpDeviation", 5);
            builder.SetParameter("InpBackstep", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-ZigZag.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResZZ.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResZZ.Count;
            System.IO.File.WriteAllLines(@"D:\out.txt", testResZZ.Select(b => ((b == 0.0 ? 0.0 : (b * 100 - 110)).ToString())).ToArray());

            for (int testInd = 100; testInd < bidsLen-100; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResZZ[testInd] - metaResZZ[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResZZ = new List<double>();
            testResZZ = new List<double>();



            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtZigzagBuffer", testResZZ);


            builder = new IndicatorBuilder<Bar>(typeof(ZigZag.ZigZag), reader, writer);
            builder.SetParameter("InpDepth", 12);
            builder.SetParameter("InpDeviation", 5);
            builder.SetParameter("InpBackstep", 3);


            builder.Build();
            
            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-ZigZag.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResZZ.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);    
            }

            int bidsLen = metaResZZ.Count;
            System.IO.File.WriteAllLines(@"D:\out.txt", testResZZ.Select(b => ((b==0.0?0.0:(b*100-110)).ToString())).ToArray());
           
            for (int testInd = 100; testInd < bidsLen-100; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResZZ[testInd] - metaResZZ[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResZZ = new List<double>();
            testResZZ = new List<double>();



            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtZigzagBuffer", testResZZ);


            builder = new IndicatorBuilder<Bar>(typeof(ZigZag.ZigZag), reader, writer);
            builder.SetParameter("InpDepth", 12);
            builder.SetParameter("InpDeviation", 5);
            builder.SetParameter("InpBackstep", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-ZigZag.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResZZ.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResZZ.Count;
            System.IO.File.WriteAllLines(@"D:\out.txt", testResZZ.Select(b => ((b == 0.0 ? 0.0 : (b * 100 - 110)).ToString())).ToArray());

            for (int testInd = 100; testInd < bidsLen - 100; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResZZ[testInd] - metaResZZ[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResZZ = new List<double>();
            testResZZ = new List<double>();



            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtZigzagBuffer", testResZZ);


            builder = new IndicatorBuilder<Bar>(typeof(ZigZag.ZigZag), reader, writer);
            builder.SetParameter("InpDepth", 12);
            builder.SetParameter("InpDeviation", 5);
            builder.SetParameter("InpBackstep", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-ZigZag.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResZZ.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResZZ.Count;
            System.IO.File.WriteAllLines(@"D:\out.txt", testResZZ.Select(b => ((b == 0.0 ? 0.0 : (b * 100 - 110)).ToString())).ToArray());

            for (int testInd = 100; testInd < bidsLen - 100; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResZZ[testInd] - metaResZZ[testInd]));
            }
        }

    }
}

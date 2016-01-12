using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.OsMATest
{
    [TestClass]
    public class OsMATest
    {

        private StreamReader file;
        private List<double> metaResOsMA;
        private List<double> testResOsMA;


        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResOsMA = new List<double>();
            testResOsMA = new List<double>();
            

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtOsmaBuffer", testResOsMA);


            builder = new IndicatorBuilder<Bar>(typeof(OsMA.OsMA), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-OsMA.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResOsMA.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResOsMA.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-8, Math.Abs(testResOsMA[testInd] - metaResOsMA[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResOsMA = new List<double>();
            testResOsMA = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtOsmaBuffer", testResOsMA);


            builder = new IndicatorBuilder<Bar>(typeof(OsMA.OsMA), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-OsMA.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResOsMA.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResOsMA.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-8, Math.Abs(testResOsMA[testInd] - metaResOsMA[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResOsMA = new List<double>();
            testResOsMA = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtOsmaBuffer", testResOsMA);


            builder = new IndicatorBuilder<Bar>(typeof(OsMA.OsMA), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-OsMA.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResOsMA.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResOsMA.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-5, Math.Abs(testResOsMA[testInd] - metaResOsMA[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResOsMA = new List<double>();
            testResOsMA = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtOsmaBuffer", testResOsMA);


            builder = new IndicatorBuilder<Bar>(typeof(OsMA.OsMA), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-OsMA.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResOsMA.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResOsMA.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-5, Math.Abs(testResOsMA[testInd] - metaResOsMA[testInd]));
            }
        }


    }
}

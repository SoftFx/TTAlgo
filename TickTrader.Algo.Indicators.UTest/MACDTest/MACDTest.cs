using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.MACDTest
{
    [TestClass]
    public class MACDTest
    {

        private StreamReader file;
        private List<double> metaResMACD;
        private List<double> testResMACD;
        private List<double> metaResSig;
        private List<double> testResSig;

        private DirectReader<Api.Bar> reader;
        private DirectWriter<Api.Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



       [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResMACD = new List<double>();
            testResMACD = new List<double>();
            metaResSig = new List<double>();
            testResSig = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMacdBuffer", testResMACD);
            writer.AddMapping("ExtSignalBuffer", testResSig);

            builder = new IndicatorBuilder<Api.Bar>(typeof(MACD.MACD), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-MACD.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMACD.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSig.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMACD.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-8, Math.Abs(testResMACD[testInd] - metaResMACD[testInd]));
                AssertX.Greater(1e-8, Math.Abs(testResSig[testInd] - metaResSig[testInd]));

            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResMACD = new List<double>();
            testResMACD = new List<double>();
            metaResSig = new List<double>();
            testResSig = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMacdBuffer", testResMACD);
            writer.AddMapping("ExtSignalBuffer", testResSig);

            builder = new IndicatorBuilder<Api.Bar>(typeof(MACD.MACD), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-MACD.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMACD.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSig.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMACD.Count;
            for (int testInd = 250; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-8, Math.Abs(testResMACD[testInd] - metaResMACD[testInd]));
                AssertX.Greater(1e-8, Math.Abs(testResSig[testInd] - metaResSig[testInd]));

            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResMACD = new List<double>();
            testResMACD = new List<double>();
            metaResSig = new List<double>();
            testResSig = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMacdBuffer", testResMACD);
            writer.AddMapping("ExtSignalBuffer", testResSig);

            builder = new IndicatorBuilder<Api.Bar>(typeof(MACD.MACD), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-MACD.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMACD.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSig.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMACD.Count;
            for (int testInd = 350; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResMACD[testInd] - metaResMACD[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResSig[testInd] - metaResSig[testInd]));

            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResMACD = new List<double>();
            testResMACD = new List<double>();
            metaResSig = new List<double>();
            testResSig = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMacdBuffer", testResMACD);
            writer.AddMapping("ExtSignalBuffer", testResSig);

            builder = new IndicatorBuilder<Api.Bar>(typeof(MACD.MACD), reader, writer);
            builder.SetParameter("InpFastEMA", 12);
            builder.SetParameter("InpSlowEMA", 26);
            builder.SetParameter("InpSignalSMA", 9);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-MACD.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMACD.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSig.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMACD.Count;
            for (int testInd = 350; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResMACD[testInd] - metaResMACD[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResSig[testInd] - metaResSig[testInd]));

            }
        }



    }
}

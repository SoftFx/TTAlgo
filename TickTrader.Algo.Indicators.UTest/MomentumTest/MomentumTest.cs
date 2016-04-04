using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Indicators.Momentum;
namespace TickTrader.Algo.Indicators.UTest.MomentumTest
{

    [TestClass]
    public class MomentumTest
    {

        private StreamReader file;
        private List<double> metaRes;
        private List<double> testRes;
        private DirectReader<Api.Bar> reader;
        private DirectWriter<Api.Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Api.Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14);

            builder.Build();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Momentum.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaRes.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testRes.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Api.Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14);

            builder.Build();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Momentum.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaRes.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testRes.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Api.Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14);

            builder.Build();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Momentum.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaRes.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testRes.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Api.Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14);

            builder.Build();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Momentum.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaRes.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testRes.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

    }
}

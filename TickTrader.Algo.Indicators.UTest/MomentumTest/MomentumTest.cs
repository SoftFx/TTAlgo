using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Indicators.Momentum;
namespace TickTrader.Algo.Indicators.UTest.MomentumTest
{

    [TestFixture]
    class MomentumTest
    {

        private StreamReader file;
        private List<double> metaRes;
        private List<double> testRes;
        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;

        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14.0);

            builder.ReadAllAndBuild();


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
                Assert.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14.0);

            builder.ReadAllAndBuild();


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
                Assert.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14.0);

            builder.ReadAllAndBuild();


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
                Assert.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaRes = new List<double>();
            testRes = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMomBuffer", testRes);

            builder = new IndicatorBuilder<Bar>(typeof(Momentum.Momentum), reader, writer);
            builder.SetParameter("Period", 14.0);

            builder.ReadAllAndBuild();


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
                Assert.Greater(1e-10, Math.Abs(testRes[testInd] - metaRes[testInd]));
            }
        }

    }
}

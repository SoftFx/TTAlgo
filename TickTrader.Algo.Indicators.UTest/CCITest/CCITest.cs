using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.CCITest
{
    [TestFixture]
    class CCITest
    {

        private StreamReader file;
        private List<double> metaResCCI;
        private List<double> testResCCI;

        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-CCI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResCCI.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResCCI.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }

        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-CCI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResCCI.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResCCI.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }
        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-CCI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResCCI.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResCCI.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }
        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-CCI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResCCI.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResCCI.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }

    }
}

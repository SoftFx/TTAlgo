using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.CCITest
{
    [TestClass]
    public class CCITest
    {
        private StreamReader file;
        private List<double> metaResCCI;
        private List<double> testResCCI;

        private DirectReader<Api.Bar> reader;
        private DirectWriter<Api.Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;


        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Api.Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.Build();

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
                AssertX.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Api.Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.Build();

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
                AssertX.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }
        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Api.Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.Build();

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
                AssertX.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }
        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResCCI = new List<double>();
            testResCCI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtCCIBuffer", testResCCI);

            builder = new IndicatorBuilder<Api.Bar>(typeof(CCI.CCI), reader, writer);
            builder.SetParameter("CCIPeriod", 14);


            builder.Build();

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
                AssertX.Greater(1e-10, Math.Abs(testResCCI[testInd] - metaResCCI[testInd]));

            }
        }

    }
}

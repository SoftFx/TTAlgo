using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.AwesomeTest
{
    [TestFixture]
    class AwesomeTest
    {

        private StreamReader file;
        private List<double> metaResAO;
        private List<double> testResAO;
        private List<double> metaResUp;
        private List<double> testResUp;
        private List<double> metaResDn;
        private List<double> testResDn;
        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResAO = new List<double>();
            testResAO = new List<double>();
            metaResUp = new List<double>();
            testResUp = new List<double>();
            metaResDn = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtAOBuffer", testResAO);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Awesome.Awesome), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Awesome.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResUp.Add(Convert.ToDouble(splitResStr[1]));
                metaResDn.Add(Convert.ToDouble(splitResStr[2]));
            }

            int bidsLen = testResAO.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                Assert.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }

        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResAO = new List<double>();
            testResAO = new List<double>();
            metaResUp = new List<double>();
            testResUp = new List<double>();
            metaResDn = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtAOBuffer", testResAO);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Awesome.Awesome), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Awesome.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResUp.Add(Convert.ToDouble(splitResStr[1]));
                metaResDn.Add(Convert.ToDouble(splitResStr[2]));
            }

            int bidsLen = testResAO.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                Assert.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }

        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResAO = new List<double>();
            testResAO = new List<double>();
            metaResUp = new List<double>();
            testResUp = new List<double>();
            metaResDn = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtAOBuffer", testResAO);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Awesome.Awesome), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Awesome.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResUp.Add(Convert.ToDouble(splitResStr[1]));
                metaResDn.Add(Convert.ToDouble(splitResStr[2]));
            }

            int bidsLen = testResAO.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                Assert.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }

        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResAO = new List<double>();
            testResAO = new List<double>();
            metaResUp = new List<double>();
            testResUp = new List<double>();
            metaResDn = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtAOBuffer", testResAO);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Awesome.Awesome), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Awesome.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResUp.Add(Convert.ToDouble(splitResStr[1]));
                metaResDn.Add(Convert.ToDouble(splitResStr[2]));
            }

            int bidsLen = testResAO.Count;
            for (int testInd = 40; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                Assert.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }
     
    }
}

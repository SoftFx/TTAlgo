using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.BearsTest
{
    [TestFixture]
    class BearsTest
    {

        private StreamReader file;
        private List<double> metaResBB;
        private List<double> testResBB;

        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResBB = new List<double>();
            testResBB = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtBearsBuffer", testResBB);

            builder = new IndicatorBuilder<Bar>(typeof(Bears.Bears), reader, writer);
            builder.SetParameter("BearsPeriod", 13);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Bears.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResBB.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResBB.Count;
            for (int testInd = 150; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResBB[testInd] - metaResBB[testInd]));

            }
        }
        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResBB = new List<double>();
            testResBB = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtBearsBuffer", testResBB);

            builder = new IndicatorBuilder<Bar>(typeof(Bears.Bears), reader, writer);
            builder.SetParameter("BearsPeriod", 13);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Bears.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResBB.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResBB.Count;
            for (int testInd = 150; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResBB[testInd] - metaResBB[testInd]));

            }
        }
        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResBB = new List<double>();
            testResBB = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtBearsBuffer", testResBB);

            builder = new IndicatorBuilder<Bar>(typeof(Bears.Bears), reader, writer);
            builder.SetParameter("BearsPeriod", 13);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Bears.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResBB.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResBB.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResBB[testInd] - metaResBB[testInd]));

            }
        }
        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResBB = new List<double>();
            testResBB = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtBearsBuffer", testResBB);

            builder = new IndicatorBuilder<Bar>(typeof(Bears.Bears), reader, writer);
            builder.SetParameter("BearsPeriod", 13);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Bears.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResBB.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResBB.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResBB[testInd] - metaResBB[testInd]));

            }
        }


    }
}

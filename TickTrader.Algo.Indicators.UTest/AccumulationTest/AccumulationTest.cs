using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.AccumulationTest
{
    [TestFixture]
    class AccumulationTest
    {

        private StreamReader file;
        private List<double> metaResAD;
        private List<double> testResAD;
        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResAD = new List<double>();
            testResAD = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtADBuffer", testResAD);


            builder = new IndicatorBuilder<Bar>(typeof(Accumulation.Accumulation), reader, writer);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Accumulator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAD.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResAD.Count;
            for (int testInd = 1; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs((testResAD[testInd]-testResAD[0]) - (metaResAD[testInd]-metaResAD[0]))); //Here we did norming by subtraction [0] element
            }
        }

        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResAD = new List<double>();
            testResAD = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtADBuffer", testResAD);


            builder = new IndicatorBuilder<Bar>(typeof(Accumulation.Accumulation), reader, writer);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Accumulator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAD.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResAD.Count;
            for (int testInd = 1; testInd < bidsLen; testInd++)
            {
                Assert.Greater(2e-10, Math.Abs((testResAD[testInd] - testResAD[0]) - (metaResAD[testInd] - metaResAD[0]))); //Here we did norming by subtraction [0] element
            }
        }


        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResAD = new List<double>();
            testResAD = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtADBuffer", testResAD);


            builder = new IndicatorBuilder<Bar>(typeof(Accumulation.Accumulation), reader, writer);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Accumulator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAD.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResAD.Count;
            for (int testInd = 1; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResAD[testInd] - testResAD[0] - metaResAD[testInd] + metaResAD[0])); //Here we did norming by subtraction [0] element
            }
        }

        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResAD = new List<double>();
            testResAD = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtADBuffer", testResAD);


            builder = new IndicatorBuilder<Bar>(typeof(Accumulation.Accumulation), reader, writer);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Accumulator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAD.Add(Convert.ToDouble(splitResStr[1]));
            }

            int bidsLen = testResAD.Count;
            for (int testInd = 1; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResAD[testInd] - testResAD[0] - metaResAD[testInd] + metaResAD[0])); //Here we did norming by subtraction [0] element
            }
        }
      
    }
}

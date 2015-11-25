using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.ATRTest
{
    [TestFixture]
    class ATRTest 
    {

        private StreamReader file;
        private List<double> metaResATR;
        private List<double> testResATR;
        private StreamReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [Test]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResATR = new List<double>();
            testResATR = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtATRBuffer", testResATR);


            builder = new IndicatorBuilder<Bar>(typeof(ATR.ATR), reader, writer);
            builder.SetParameter("InpAtrPeriod", 14);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-ATR.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResATR.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResATR.Add(Double.NaN);
            }

            int bidsLen = testResATR.Count;
            for (int testInd = 20; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResATR[testInd] - metaResATR[testInd]));
            }
        }



        [Test]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResATR = new List<double>();
            testResATR = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtATRBuffer", testResATR);


            builder = new IndicatorBuilder<Bar>(typeof(ATR.ATR), reader, writer);
            builder.SetParameter("InpAtrPeriod", 14);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-ATR.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResATR.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResATR.Add(Double.NaN);
            }

            int bidsLen = testResATR.Count;
            for (int testInd = 20; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResATR[testInd] - metaResATR[testInd]));
            }
        }




        [Test]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResATR = new List<double>();
            testResATR = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtATRBuffer", testResATR);


            builder = new IndicatorBuilder<Bar>(typeof(ATR.ATR), reader, writer);
            builder.SetParameter("InpAtrPeriod", 14);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-ATR.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResATR.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResATR.Add(Double.NaN);
            }

            int bidsLen = testResATR.Count;
            for (int testInd = 20; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResATR[testInd] - metaResATR[testInd]));
            }
        }




        [Test]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResATR = new List<double>();
            testResATR = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtATRBuffer", testResATR);


            builder = new IndicatorBuilder<Bar>(typeof(ATR.ATR), reader, writer);
            builder.SetParameter("InpAtrPeriod", 14);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-ATR.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResATR.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResATR.Add(Double.NaN);
            }

            int bidsLen = testResATR.Count;
            for (int testInd = 20; testInd < bidsLen; testInd++)
            {
                Assert.Greater(1e-10, Math.Abs(testResATR[testInd] - metaResATR[testInd]));
            }
        }


    }
}

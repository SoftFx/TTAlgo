using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.HeikenAshiTest
{
    [TestClass]
    public class HeikenAshiTest
    {

        private StreamReader file;
        private List<double> metaResHAHL;
        private List<double> testResHAHL;
        private List<double> metaResHALH;
        private List<double> testResHALH;
        private List<double> metaResHAOpen;
        private List<double> testResHAOpen;
        private List<double> metaResHAClose;
        private List<double> testResHAClose;
        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResHAHL = new List<double>();
            testResHAHL = new List<double>();
            metaResHALH = new List<double>();
            testResHALH = new List<double>();
            metaResHAOpen = new List<double>();
            testResHAOpen = new List<double>();
            metaResHAClose = new List<double>();
            testResHAClose = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtLowHighBuffer", testResHALH);
            writer.AddMapping("ExtHighLowBuffer", testResHAHL);
            writer.AddMapping("ExtOpenBuffer", testResHAOpen);
            writer.AddMapping("ExtCloseBuffer", testResHAClose);

            builder = new IndicatorBuilder<Bar>(typeof(HeikenAshi.HeikenAshi), reader, writer);



            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Heiken Ashi.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResHALH.Add(Convert.ToDouble(splitResStr[1]));
                metaResHAHL.Add(Convert.ToDouble(splitResStr[2]));
                metaResHAOpen.Add(Convert.ToDouble(splitResStr[3]));
                metaResHAClose.Add(Convert.ToDouble(splitResStr[4]));
            }

            int bidsLen = testResHAHL.Count;
            for (int testInd = 35; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(testResHALH[testInd] - metaResHALH[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAHL[testInd] - metaResHAHL[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAOpen[testInd] - metaResHAOpen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAClose[testInd] - metaResHAClose[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResHAHL = new List<double>();
            testResHAHL = new List<double>();
            metaResHALH = new List<double>();
            testResHALH = new List<double>();
            metaResHAOpen = new List<double>();
            testResHAOpen = new List<double>();
            metaResHAClose = new List<double>();
            testResHAClose = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtLowHighBuffer", testResHALH);
            writer.AddMapping("ExtHighLowBuffer", testResHAHL);
            writer.AddMapping("ExtOpenBuffer", testResHAOpen);
            writer.AddMapping("ExtCloseBuffer", testResHAClose);

            builder = new IndicatorBuilder<Bar>(typeof(HeikenAshi.HeikenAshi), reader, writer);



            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Heiken Ashi.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResHALH.Add(Convert.ToDouble(splitResStr[1]));
                metaResHAHL.Add(Convert.ToDouble(splitResStr[2]));
                metaResHAOpen.Add(Convert.ToDouble(splitResStr[3]));
                metaResHAClose.Add(Convert.ToDouble(splitResStr[4]));
            }

            int bidsLen = testResHAHL.Count;
            for (int testInd = 35; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(testResHALH[testInd] - metaResHALH[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAHL[testInd] - metaResHAHL[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAOpen[testInd] - metaResHAOpen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAClose[testInd] - metaResHAClose[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResHAHL = new List<double>();
            testResHAHL = new List<double>();
            metaResHALH = new List<double>();
            testResHALH = new List<double>();
            metaResHAOpen = new List<double>();
            testResHAOpen = new List<double>();
            metaResHAClose = new List<double>();
            testResHAClose = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtLowHighBuffer", testResHALH);
            writer.AddMapping("ExtHighLowBuffer", testResHAHL);
            writer.AddMapping("ExtOpenBuffer", testResHAOpen);
            writer.AddMapping("ExtCloseBuffer", testResHAClose);

            builder = new IndicatorBuilder<Bar>(typeof(HeikenAshi.HeikenAshi), reader, writer);



            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Heiken Ashi.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResHALH.Add(Convert.ToDouble(splitResStr[1]));
                metaResHAHL.Add(Convert.ToDouble(splitResStr[2]));
                metaResHAOpen.Add(Convert.ToDouble(splitResStr[3]));
                metaResHAClose.Add(Convert.ToDouble(splitResStr[4]));
            }

            int bidsLen = testResHAHL.Count;
            for (int testInd = 35; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(testResHALH[testInd] - metaResHALH[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAHL[testInd] - metaResHAHL[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAOpen[testInd] - metaResHAOpen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAClose[testInd] - metaResHAClose[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResHAHL = new List<double>();
            testResHAHL = new List<double>();
            metaResHALH = new List<double>();
            testResHALH = new List<double>();
            metaResHAOpen = new List<double>();
            testResHAOpen = new List<double>();
            metaResHAClose = new List<double>();
            testResHAClose = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtLowHighBuffer", testResHALH);
            writer.AddMapping("ExtHighLowBuffer", testResHAHL);
            writer.AddMapping("ExtOpenBuffer", testResHAOpen);
            writer.AddMapping("ExtCloseBuffer", testResHAClose);

            builder = new IndicatorBuilder<Bar>(typeof(HeikenAshi.HeikenAshi), reader, writer);



            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Heiken Ashi.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResHALH.Add(Convert.ToDouble(splitResStr[1]));
                metaResHAHL.Add(Convert.ToDouble(splitResStr[2]));
                metaResHAOpen.Add(Convert.ToDouble(splitResStr[3]));
                metaResHAClose.Add(Convert.ToDouble(splitResStr[4]));
            }

            int bidsLen = testResHAHL.Count;
            for (int testInd = 35; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(testResHALH[testInd] - metaResHALH[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAHL[testInd] - metaResHAHL[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAOpen[testInd] - metaResHAOpen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResHAClose[testInd] - metaResHAClose[testInd]));
            }
        }



    }
}

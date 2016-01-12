using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.BandsTest
{
    [TestClass]
    public class BandsTest
    {

        private StreamReader file;
        private List<double> metaResMov;
        private List<double> metaResUp;
        private List<double> metaResLw;
        private List<double> testResMov;
        private List<double> testResUp;
        private List<double> testResLw;
        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResMov = new List<double>();
            metaResUp = new List<double>();
            metaResLw = new List<double>();
            testResMov = new List<double>();
            testResUp = new List<double>();
            testResLw = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMovingBuffer", testResMov);
            writer.AddMapping("ExtUpperBuffer", testResUp);
            writer.AddMapping("ExtLowerBuffer", testResLw);

            builder = new IndicatorBuilder<Bar>(typeof(Bands.Bands), reader, writer);
            builder.SetParameter("Period", 20);
            builder.SetParameter("Shift", 0.0);
            builder.SetParameter("Deviations", 2.0);
  
            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Bands.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMov.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResLw.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResMov.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(0.000001, Math.Abs(testResMov[testInd] - metaResMov[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResLw[testInd] - metaResLw[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResMov = new List<double>();
            metaResUp = new List<double>();
            metaResLw = new List<double>();
            testResMov = new List<double>();
            testResUp = new List<double>();
            testResLw = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMovingBuffer", testResMov);
            writer.AddMapping("ExtUpperBuffer", testResUp);
            writer.AddMapping("ExtLowerBuffer", testResLw);

            builder = new IndicatorBuilder<Bar>(typeof(Bands.Bands), reader, writer);
            builder.SetParameter("Period", 20);
            builder.SetParameter("Shift", 0.0);
            builder.SetParameter("Deviations", 2.0);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Bands.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMov.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResLw.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResMov.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(0.000001, Math.Abs(testResMov[testInd] - metaResMov[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResLw[testInd] - metaResLw[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResMov = new List<double>();
            metaResUp = new List<double>();
            metaResLw = new List<double>();
            testResMov = new List<double>();
            testResUp = new List<double>();
            testResLw = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMovingBuffer", testResMov);
            writer.AddMapping("ExtUpperBuffer", testResUp);
            writer.AddMapping("ExtLowerBuffer", testResLw);

            builder = new IndicatorBuilder<Bar>(typeof(Bands.Bands), reader, writer);
            builder.SetParameter("Period", 20);
            builder.SetParameter("Shift", 0.0);
            builder.SetParameter("Deviations", 2.0);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Bands.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMov.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResLw.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResMov.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(0.000001, Math.Abs(testResMov[testInd] - metaResMov[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResLw[testInd] - metaResLw[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResMov = new List<double>();
            metaResUp = new List<double>();
            metaResLw = new List<double>();
            testResMov = new List<double>();
            testResUp = new List<double>();
            testResLw = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Close", b => b.Close);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMovingBuffer", testResMov);
            writer.AddMapping("ExtUpperBuffer", testResUp);
            writer.AddMapping("ExtLowerBuffer", testResLw);

            builder = new IndicatorBuilder<Bar>(typeof(Bands.Bands), reader, writer);
            builder.SetParameter("Period", 20);
            builder.SetParameter("Shift", 0.0);
            builder.SetParameter("Deviations", 2.0);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Bands.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMov.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResLw.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResMov.Count;
            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                AssertX.Greater(0.000001, Math.Abs(testResMov[testInd] - metaResMov[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(0.000001, Math.Abs(testResLw[testInd] - metaResLw[testInd]));
            }
        }

    }
}

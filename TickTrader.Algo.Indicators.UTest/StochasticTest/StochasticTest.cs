using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.StochasticTest
{
    [TestClass]
    public class StochasticTest
    {

        private StreamReader file;
        private List<double> metaResMn;
        private List<double> testResMn;
        private List<double> metaResSg;
        private List<double> testResSg;

        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResMn = new List<double>();
            testResMn = new List<double>();
            metaResSg = new List<double>();
            testResSg = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMainBuffer", testResMn);
            writer.AddMapping("ExtSignalBuffer", testResSg);

            builder = new IndicatorBuilder<Bar>(typeof(Stochastic.Stochastic), reader, writer);
            builder.SetParameter("InpKPeriod", 5);
            builder.SetParameter("InpDPeriod", 3);
            builder.SetParameter("InpSlowing", 3);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Stochastic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMn.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSg.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMn.Count;
            for (int testInd = 15; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResMn[testInd] - metaResMn[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResSg[testInd] - metaResSg[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResMn = new List<double>();
            testResMn = new List<double>();
            metaResSg = new List<double>();
            testResSg = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMainBuffer", testResMn);
            writer.AddMapping("ExtSignalBuffer", testResSg);

            builder = new IndicatorBuilder<Bar>(typeof(Stochastic.Stochastic), reader, writer);
            builder.SetParameter("InpKPeriod", 5);
            builder.SetParameter("InpDPeriod", 3);
            builder.SetParameter("InpSlowing", 3);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Stochastic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMn.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSg.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMn.Count;
            for (int testInd = 15; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResMn[testInd] - metaResMn[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResSg[testInd] - metaResSg[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResMn = new List<double>();
            testResMn = new List<double>();
            metaResSg = new List<double>();
            testResSg = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMainBuffer", testResMn);
            writer.AddMapping("ExtSignalBuffer", testResSg);

            builder = new IndicatorBuilder<Bar>(typeof(Stochastic.Stochastic), reader, writer);
            builder.SetParameter("InpKPeriod", 5);
            builder.SetParameter("InpDPeriod", 3);
            builder.SetParameter("InpSlowing", 3);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Stochastic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMn.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSg.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMn.Count;
            for (int testInd = 15; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResMn[testInd] - metaResMn[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResSg[testInd] - metaResSg[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResMn = new List<double>();
            testResMn = new List<double>();
            metaResSg = new List<double>();
            testResSg = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtMainBuffer", testResMn);
            writer.AddMapping("ExtSignalBuffer", testResSg);

            builder = new IndicatorBuilder<Bar>(typeof(Stochastic.Stochastic), reader, writer);
            builder.SetParameter("InpKPeriod", 5);
            builder.SetParameter("InpDPeriod", 3);
            builder.SetParameter("InpSlowing", 3);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Stochastic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResMn.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResSg.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
            }

            int bidsLen = metaResMn.Count;
            for (int testInd = 15; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResMn[testInd] - metaResMn[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResSg[testInd] - metaResSg[testInd]));
            }
        }


    }
}

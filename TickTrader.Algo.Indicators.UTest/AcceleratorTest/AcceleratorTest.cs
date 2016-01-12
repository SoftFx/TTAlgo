using System;
using System.IO;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.AcceleratorTest
{
    [TestClass]
    public class AcceleratorTest
    {
        private StreamReader file;
        private List<double> metaResAc;
        private List<double> metaResUp;
        private List<double> metaResDn;
        private List<double> testResAc;
        private List<double> testResUp;
        private List<double> testResDn;
        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResAc = new List<double>();
            metaResUp = new List<double>();
            metaResDn = new List<double>();
            testResAc = new List<double>();
            testResUp = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtACBuffer", testResAc);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Accelerator.Accelerator), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);
            builder.SetParameter("DataLimit", 38);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Accelerator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAc.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResDn.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResAc.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResAc[testInd] - metaResAc[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResAc = new List<double>();
            metaResUp = new List<double>();
            metaResDn = new List<double>();
            testResAc = new List<double>();
            testResUp = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtACBuffer", testResAc);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Accelerator.Accelerator), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);
            builder.SetParameter("DataLimit", 38);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Accelerator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAc.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResDn.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResAc.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResAc[testInd] - metaResAc[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResAc = new List<double>();
            metaResUp = new List<double>();
            metaResDn = new List<double>();
            testResAc = new List<double>();
            testResUp = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtACBuffer", testResAc);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Accelerator.Accelerator), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);
            builder.SetParameter("DataLimit", 38);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Accelerator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAc.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResDn.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResAc.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResAc[testInd] - metaResAc[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResAc = new List<double>();
            metaResUp = new List<double>();
            metaResDn = new List<double>();
            testResAc = new List<double>();
            testResUp = new List<double>();
            testResDn = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtACBuffer", testResAc);
            writer.AddMapping("ExtUpBuffer", testResUp);
            writer.AddMapping("ExtDnBuffer", testResDn);

            builder = new IndicatorBuilder<Bar>(typeof(Accelerator.Accelerator), reader, writer);
            builder.SetParameter("PeriodFast", 5);
            builder.SetParameter("PeriodSlow", 34);
            builder.SetParameter("DataLimit", 38);

            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Accelerator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResAc.Add(Convert.ToDouble(splitResStr[1]));
                metaResUp.Add(Convert.ToDouble(splitResStr[2]));
                metaResDn.Add(Convert.ToDouble(splitResStr[3]));
            }

            int bidsLen = testResAc.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResAc[testInd] - metaResAc[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResUp[testInd] - metaResUp[testInd]));
                AssertX.Greater(1e-10, Math.Abs(testResDn[testInd] - metaResDn[testInd]));
            }
        }
        
    }
}

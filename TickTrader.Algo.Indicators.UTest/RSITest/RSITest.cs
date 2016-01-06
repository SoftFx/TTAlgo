using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.RSITest
{
    [TestClass]
    public class RSITest
    {

        private StreamReader file;
        private List<double> metaResRSI;
        private List<double> testResRSI;


        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResRSI = new List<double>();
            testResRSI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtRSIBuffer", testResRSI);


            builder = new IndicatorBuilder<Bar>(typeof(RSI.RSI), reader, writer);
            builder.SetParameter("InpRSIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-RSI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResRSI.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResRSI.Count;
            for (int testInd = 400; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResRSI[testInd] - metaResRSI[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResRSI = new List<double>();
            testResRSI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtRSIBuffer", testResRSI);


            builder = new IndicatorBuilder<Bar>(typeof(RSI.RSI), reader, writer);
            builder.SetParameter("InpRSIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-RSI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResRSI.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResRSI.Count;
            for (int testInd = 400; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResRSI[testInd] - metaResRSI[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResRSI = new List<double>();
            testResRSI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtRSIBuffer", testResRSI);


            builder = new IndicatorBuilder<Bar>(typeof(RSI.RSI), reader, writer);
            builder.SetParameter("InpRSIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-RSI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResRSI.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResRSI.Count;
            for (int testInd = 400; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResRSI[testInd] - metaResRSI[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResRSI = new List<double>();
            testResRSI = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtRSIBuffer", testResRSI);


            builder = new IndicatorBuilder<Bar>(typeof(RSI.RSI), reader, writer);
            builder.SetParameter("InpRSIPeriod", 14);


            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-RSI.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResRSI.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResRSI.Count;
            for (int testInd = 400; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-6, Math.Abs(testResRSI[testInd] - metaResRSI[testInd]));
            }
        }






    }
}

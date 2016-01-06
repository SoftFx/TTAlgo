using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.PorabolicTest
{
    [TestClass]
    public class PorabolicTest
    {

        private StreamReader file;
        private List<double> metaResPor;
        private List<double> testResPor;


        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResPor = new List<double>();
            testResPor = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtSARBuffer", testResPor);


            builder = new IndicatorBuilder<Bar>(typeof(Porabolic.Porabolic), reader, writer);
            builder.SetParameter("InpSARStep", 0.02);
            builder.SetParameter("InpSARMaximum", 0.2);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Parabolic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResPor.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResPor.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResPor[testInd] - metaResPor[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResPor = new List<double>();
            testResPor = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtSARBuffer", testResPor);


            builder = new IndicatorBuilder<Bar>(typeof(Porabolic.Porabolic), reader, writer);
            builder.SetParameter("InpSARStep", 0.02);
            builder.SetParameter("InpSARMaximum", 0.2);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Parabolic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResPor.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResPor.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResPor[testInd] - metaResPor[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResPor = new List<double>();
            testResPor = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtSARBuffer", testResPor);


            builder = new IndicatorBuilder<Bar>(typeof(Porabolic.Porabolic), reader, writer);
            builder.SetParameter("InpSARStep", 0.02);
            builder.SetParameter("InpSARMaximum", 0.2);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Parabolic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResPor.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResPor.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResPor[testInd] - metaResPor[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResPor = new List<double>();
            testResPor = new List<double>();


            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtSARBuffer", testResPor);


            builder = new IndicatorBuilder<Bar>(typeof(Porabolic.Porabolic), reader, writer);
            builder.SetParameter("InpSARStep", 0.02);
            builder.SetParameter("InpSARMaximum", 0.2);



            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Parabolic.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResPor.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
            }

            int bidsLen = metaResPor.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {
                AssertX.Greater(1e-10, Math.Abs(testResPor[testInd] - metaResPor[testInd]));
            }
        }



    }
}

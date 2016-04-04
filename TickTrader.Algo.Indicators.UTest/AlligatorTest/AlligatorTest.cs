using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.AlligatorTest
{
    [TestClass]
    public class AlligatorTest
    {

        private StreamReader file;
        private List<double> metaResBlue;
        private List<double> testResBlue;
        private List<double> metaResRed;
        private List<double> testResRed;
        private List<double> metaResLime;
        private List<double> testResLime;
        private DirectReader<Api.Bar> reader;
        private DirectWriter<Api.Bar> writer;
        private IndicatorBuilder<Api.Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResBlue = new List<double>();
            testResBlue = new List<double>();
            metaResRed = new List<double>();
            testResRed = new List<double>();
            metaResLime = new List<double>();
            testResLime = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtBlueBuffer", testResBlue);
            writer.AddMapping("ExtRedBuffer", testResRed);
            writer.AddMapping("ExtLimeBuffer", testResLime);


            builder = new IndicatorBuilder<Api.Bar>(typeof(Alligator.Alligator), reader, writer);
            builder.SetParameter("InpJawsPeriod", 13);
            builder.SetParameter("InpJawsShift", 8);
            builder.SetParameter("InpTeethPeriod", 8);
            builder.SetParameter("InpTeethShift", 5);
            builder.SetParameter("InpLipsPeriod", 5);
            builder.SetParameter("InpLipsShift", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Alligator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResBlue.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResBlue.Add(Double.NaN);

                if (splitResStr[2] != "")
                    metaResRed.Add(Convert.ToDouble(splitResStr[2]));
                else
                    metaResRed.Add(Double.NaN);

                if (splitResStr[3] != "")
                    metaResLime.Add(Convert.ToDouble(splitResStr[3]));
                else
                    metaResLime.Add(Double.NaN);
            }

            int bidsLen = testResBlue.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                double a = testResBlue[testInd]*13 - testResBlue[testInd - 1]*12;
                double b = metaResBlue[testInd]*13 - metaResBlue[testInd-1]*12;
                AssertX.Greater(1e-8, Math.Abs((testResBlue[testInd]*13 - testResBlue[testInd-1]*12) - (metaResBlue[testInd]*13 - metaResBlue[testInd-1]*12)));
                AssertX.Greater(1e-7, Math.Abs(testResBlue[testInd] - metaResBlue[testInd]));

                AssertX.Greater(1e-8, Math.Abs((testResRed[testInd] * 13 - testResRed[testInd - 1] * 12) - (metaResRed[testInd] * 13 - metaResRed[testInd - 1] * 12)));
                AssertX.Greater(1e-7, Math.Abs(testResRed[testInd] - metaResRed[testInd]));

                AssertX.Greater(1e-8, Math.Abs((testResLime[testInd] * 13 - testResLime[testInd - 1] * 12) - (metaResLime[testInd] * 13 - metaResLime[testInd - 1] * 12)));
                AssertX.Greater(1e-7, Math.Abs(testResLime[testInd] - metaResLime[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResBlue = new List<double>();
            testResBlue = new List<double>();
            metaResRed = new List<double>();
            testResRed = new List<double>();
            metaResLime = new List<double>();
            testResLime = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtBlueBuffer", testResBlue);
            writer.AddMapping("ExtRedBuffer", testResRed);
            writer.AddMapping("ExtLimeBuffer", testResLime);


            builder = new IndicatorBuilder<Api.Bar>(typeof(Alligator.Alligator), reader, writer);
            builder.SetParameter("InpJawsPeriod", 13);
            builder.SetParameter("InpJawsShift", 8);
            builder.SetParameter("InpTeethPeriod", 8);
            builder.SetParameter("InpTeethShift", 5);
            builder.SetParameter("InpLipsPeriod", 5);
            builder.SetParameter("InpLipsShift", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Alligator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResBlue.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResBlue.Add(Double.NaN);

                if (splitResStr[2] != "")
                    metaResRed.Add(Convert.ToDouble(splitResStr[2]));
                else
                    metaResRed.Add(Double.NaN);

                if (splitResStr[3] != "")
                    metaResLime.Add(Convert.ToDouble(splitResStr[3]));
                else
                    metaResLime.Add(Double.NaN);
            }

            int bidsLen = testResBlue.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                double a = testResBlue[testInd] * 13 - testResBlue[testInd - 1] * 12;
                double b = metaResBlue[testInd] * 13 - metaResBlue[testInd - 1] * 12;
                AssertX.Greater(1e-8, Math.Abs((testResBlue[testInd] * 13 - testResBlue[testInd - 1] * 12) - (metaResBlue[testInd] * 13 - metaResBlue[testInd - 1] * 12)));
                AssertX.Greater(1e-7, Math.Abs(testResBlue[testInd] - metaResBlue[testInd]));

                AssertX.Greater(1e-8, Math.Abs((testResRed[testInd] * 13 - testResRed[testInd - 1] * 12) - (metaResRed[testInd] * 13 - metaResRed[testInd - 1] * 12)));
                AssertX.Greater(1e-7, Math.Abs(testResRed[testInd] - metaResRed[testInd]));

                AssertX.Greater(1e-8, Math.Abs((testResLime[testInd] * 13 - testResLime[testInd - 1] * 12) - (metaResLime[testInd] * 13 - metaResLime[testInd - 1] * 12)));
                AssertX.Greater(1e-7, Math.Abs(testResLime[testInd] - metaResLime[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResBlue = new List<double>();
            testResBlue = new List<double>();
            metaResRed = new List<double>();
            testResRed = new List<double>();
            metaResLime = new List<double>();
            testResLime = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtBlueBuffer", testResBlue);
            writer.AddMapping("ExtRedBuffer", testResRed);
            writer.AddMapping("ExtLimeBuffer", testResLime);


            builder = new IndicatorBuilder<Api.Bar>(typeof(Alligator.Alligator), reader, writer);
            builder.SetParameter("InpJawsPeriod", 13);
            builder.SetParameter("InpJawsShift", 8);
            builder.SetParameter("InpTeethPeriod", 8);
            builder.SetParameter("InpTeethShift", 5);
            builder.SetParameter("InpLipsPeriod", 5);
            builder.SetParameter("InpLipsShift", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Alligator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResBlue.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResBlue.Add(Double.NaN);

                if (splitResStr[2] != "")
                    metaResRed.Add(Convert.ToDouble(splitResStr[2]));
                else
                    metaResRed.Add(Double.NaN);

                if (splitResStr[3] != "")
                    metaResLime.Add(Convert.ToDouble(splitResStr[3]));
                else
                    metaResLime.Add(Double.NaN);
            }

            int bidsLen = testResBlue.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                double a = testResBlue[testInd] * 13 - testResBlue[testInd - 1] * 12;
                double b = metaResBlue[testInd] * 13 - metaResBlue[testInd - 1] * 12;
                AssertX.Greater(1e-6, Math.Abs(testResBlue[testInd] - metaResBlue[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResRed[testInd] - metaResRed[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResLime[testInd] - metaResLime[testInd]));
            }
        }


        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResBlue = new List<double>();
            testResBlue = new List<double>();
            metaResRed = new List<double>();
            testResRed = new List<double>();
            metaResLime = new List<double>();
            testResLime = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Api.Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Api.Bar>();
            writer.AddMapping("ExtBlueBuffer", testResBlue);
            writer.AddMapping("ExtRedBuffer", testResRed);
            writer.AddMapping("ExtLimeBuffer", testResLime);


            builder = new IndicatorBuilder<Api.Bar>(typeof(Alligator.Alligator), reader, writer);
            builder.SetParameter("InpJawsPeriod", 13);
            builder.SetParameter("InpJawsShift", 8);
            builder.SetParameter("InpTeethPeriod", 8);
            builder.SetParameter("InpTeethShift", 5);
            builder.SetParameter("InpLipsPeriod", 5);
            builder.SetParameter("InpLipsShift", 3);


            builder.Build();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Alligator.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                if (splitResStr[1] != "")
                    metaResBlue.Add(Convert.ToDouble(splitResStr[1]));
                else
                    metaResBlue.Add(Double.NaN);

                if (splitResStr[2] != "")
                    metaResRed.Add(Convert.ToDouble(splitResStr[2]));
                else
                    metaResRed.Add(Double.NaN);

                if (splitResStr[3] != "")
                    metaResLime.Add(Convert.ToDouble(splitResStr[3]));
                else
                    metaResLime.Add(Double.NaN);
            }

            int bidsLen = testResBlue.Count;
            for (int testInd = 200; testInd < bidsLen; testInd++)
            {
                double a = testResBlue[testInd] * 13 - testResBlue[testInd - 1] * 12;
                double b = metaResBlue[testInd] * 13 - metaResBlue[testInd - 1] * 12;
                AssertX.Greater(1e-6, Math.Abs(testResBlue[testInd] - metaResBlue[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResRed[testInd] - metaResRed[testInd]));
                AssertX.Greater(1e-6, Math.Abs(testResLime[testInd] - metaResLime[testInd]));
            }
        }



    }
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [TestFixture]
    class BandsTest
    {

        public class UserBands : Bands
        {
            public void Init(DataSeries close, DataSeries resMov, DataSeries resUp, DataSeries resLw)
            {
                Close = close;
                ExtMovingBuffer = resMov;
                ExtUpperBuffer = resUp;
                ExtLowerBuffer = resLw;
            }

            public void Calc()
            {
                Calculate();
            }
        }

        private UserBands bands;
        private StreamReader sr;
        private TestDataSeries inputSeries;
        private TestDataSeries outputSeriesMov;
        private TestDataSeries outputSeriesUp;
        private TestDataSeries outputSeriesLw;
        private List<double> trueResMov;
        private List<double> trueResUp;
        private List<double> trueResLw;
        private List<double> testResMov;
        private List<double> testResUp;
        private List<double> testResLw;

        [SetUp]
        public void Init()
        {
            bands = new UserBands();
            inputSeries = new TestDataSeries();
            outputSeriesMov = new TestDataSeries();
            outputSeriesUp = new TestDataSeries();
            outputSeriesLw = new TestDataSeries();
            trueResMov = new List<double>();
            trueResUp = new List<double>();
            trueResLw = new List<double>();
            testResMov = new List<double>();
            testResUp = new List<double>();
            testResLw = new List<double>();

            sr = File.OpenText(@"..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");
            string bidsStr;
            while ((bidsStr = sr.ReadLine()) != null)
            {
                string[] splitBidsStr = bidsStr.Split('\t');
                inputSeries.ser.Add(Convert.ToDouble(splitBidsStr[4]));
            }
            inputSeries.ser.Reverse();

            sr = File.OpenText(@"..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Bands.txt");
            string resStr;
            while ((resStr = sr.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                trueResMov.Add(Convert.ToDouble(splitResStr[1]));
                trueResUp.Add(Convert.ToDouble(splitResStr[2]));
                trueResLw.Add(Convert.ToDouble(splitResStr[3]));
            }

        }



        [Test]
        public void TestConstructorClassic()
        {
            int bidsLen = inputSeries.Count();

            bands.Period = 20;
            bands.Deviations = 2;

            outputSeriesMov.ser.Add(0.0);
            outputSeriesUp.ser.Add(0.0);
            outputSeriesLw.ser.Add(0.0);


            for (int i = 0; i < bidsLen; i++)
            {
                bands.Init(inputSeries, outputSeriesMov, outputSeriesUp, outputSeriesLw);
                bands.Calc();
                testResMov.Add(bands.ExtMovingBuffer[0]);
                testResUp.Add(bands.ExtUpperBuffer[0]);
                testResLw.Add(bands.ExtLowerBuffer[0]);
                inputSeries.ser.RemoveAt(0);
            }
            testResMov.Reverse();
            testResUp.Reverse();
            testResLw.Reverse();

            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                Assert.Greater(0.000001, Math.Abs(testResMov[testInd] - trueResMov[testInd]));
                Assert.Greater(0.000001, Math.Abs(testResUp[testInd] - trueResUp[testInd]));
                Assert.Greater(0.000001, Math.Abs(testResLw[testInd] - trueResLw[testInd]));
            }

        }

    }
}

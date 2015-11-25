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
    class MomentumTest
    {

        public class UserMomentum : Momentum
        {
            public void Init(DataSeries close, DataSeries res)
            {
                Close = close;
                ExtMomBuffer = res;
            }

            public void Calc()
            {
                Calculate();
            }
        }

        private UserMomentum momentum;
        private StreamReader sr;
        private TestDataSeries inputSeries;
        private TestDataSeries outputSeries;
        private List<double> trueRes;
        private List<double> testRes;
        
        [SetUp]
        public void Init()
        {
            momentum = new UserMomentum();
            inputSeries = new TestDataSeries();
            outputSeries = new TestDataSeries();
            trueRes = new List<double>();
            testRes = new List<double>();

            sr = File.OpenText(@"..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-M1-bids.txt");
            string bidsStr;
            while ((bidsStr = sr.ReadLine()) != null)
            {
                string[] splitBidsStr = bidsStr.Split('\t');
                inputSeries.ser.Add(Convert.ToDouble(splitBidsStr[4]));
            }
            inputSeries.ser.Reverse();

            sr = File.OpenText(@"..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Momentum.txt");
            string resStr;
            while ((resStr = sr.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                trueRes.Add(Convert.ToDouble(splitResStr[1]));
            }
            
        }



        [Test]
        public void TestConstructorClassic()
        {
            int bidsLen = inputSeries.Count();

            momentum.Period = 14;

            outputSeries.ser.Add(0.0);

      
            for (int i = 0; i < bidsLen; i++)
            {
                momentum.Init(inputSeries, outputSeries);
                momentum.Calc();
                testRes.Add(momentum.ExtMomBuffer[0]);
                inputSeries.ser.RemoveAt(0);
            }
            testRes.Reverse();

            for (int testInd = 20; testInd < bidsLen - 20; testInd++)
            {
                Assert.Greater(0.000001, Math.Abs(testRes[testInd] - trueRes[testInd]));
            }
           
        }

    }
}

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;


namespace TickTrader.Algo.Indicators.UTest.IchimokuTest
{
    public class IchimokuTest
    {
        private StreamReader file;

        private List<double> metaResTen;
        private List<double> testResTen;
        private List<double> metaResKij;
        private List<double> testResKij;
        private List<double> metaResSpA;
        private List<double> testResSpA;
        private List<double> metaResSpB;
        private List<double> testResSpB;
        private List<double> metaResChk;
        private List<double> testResChk;
        private List<double> metaResSpA2;
        private List<double> testResSpA2;
        private List<double> metaResSpB2;
        private List<double> testResSpB2;

        private DirectReader<Bar> reader;
        private DirectWriter<Bar> writer;
        private IndicatorBuilder<Bar> builder;



        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            metaResTen = new List<double>();
            testResTen = new List<double>();
            metaResKij = new List<double>();
            testResKij = new List<double>();
            metaResSpA = new List<double>();
            testResSpA = new List<double>();
            metaResSpB = new List<double>();
            testResSpB = new List<double>();
            metaResChk = new List<double>();
            testResChk = new List<double>();
            metaResSpA2 = new List<double>();
            testResSpA2 = new List<double>();
            metaResSpB2 = new List<double>();
            testResSpB2 = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtTenkanBuffer", testResTen);
            writer.AddMapping("ExtKijunBuffer", testResKij);
            writer.AddMapping("ExtSpanA_Buffer", testResSpA);
            writer.AddMapping("ExtSpanB_Buffer", testResSpB);
            writer.AddMapping("ExtChikouBuffer", testResChk);
            writer.AddMapping("ExtSpanA2_Buffer", testResSpA2);
            writer.AddMapping("ExtSpanB2_Buffer", testResSpB2);

            builder = new IndicatorBuilder<Bar>(typeof(Ichimoku.Ichimoku), reader, writer);
            builder.SetParameter("InpTenkan", 9);
            builder.SetParameter("InpKijun", 26);
            builder.SetParameter("InpSenkou", 52);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-Ichimoku.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResTen.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResKij.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
                metaResSpA.Add(splitResStr[3] != "" ? Convert.ToDouble(splitResStr[3]) : Double.NaN);
                metaResSpB.Add(splitResStr[4] != "" ? Convert.ToDouble(splitResStr[4]) : Double.NaN);
                metaResChk.Add(splitResStr[5] != "" ? Convert.ToDouble(splitResStr[5]) : Double.NaN);
                metaResSpA2.Add(splitResStr[6] != "" ? Convert.ToDouble(splitResStr[6]) : Double.NaN);
                metaResSpB2.Add(splitResStr[7] != "" ? Convert.ToDouble(splitResStr[7]) : Double.NaN);
            }

            int bidsLen = testResTen.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(metaResTen[testInd] - testResTen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResKij[testInd] - testResKij[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA[testInd] - testResSpA[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA2[testInd] - testResSpA2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB[testInd] - testResSpB[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB2[testInd] - testResSpB2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResChk[testInd] - testResChk[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            metaResTen = new List<double>();
            testResTen = new List<double>();
            metaResKij = new List<double>();
            testResKij = new List<double>();
            metaResSpA = new List<double>();
            testResSpA = new List<double>();
            metaResSpB = new List<double>();
            testResSpB = new List<double>();
            metaResChk = new List<double>();
            testResChk = new List<double>();
            metaResSpA2 = new List<double>();
            testResSpA2 = new List<double>();
            metaResSpB2 = new List<double>();
            testResSpB2 = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-EURUSD\EURUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtTenkanBuffer", testResTen);
            writer.AddMapping("ExtKijunBuffer", testResKij);
            writer.AddMapping("ExtSpanA_Buffer", testResSpA);
            writer.AddMapping("ExtSpanB_Buffer", testResSpB);
            writer.AddMapping("ExtChikouBuffer", testResChk);
            writer.AddMapping("ExtSpanA2_Buffer", testResSpA2);
            writer.AddMapping("ExtSpanB2_Buffer", testResSpB2);

            builder = new IndicatorBuilder<Bar>(typeof(Ichimoku.Ichimoku), reader, writer);
            builder.SetParameter("InpTenkan", 9);
            builder.SetParameter("InpKijun", 26);
            builder.SetParameter("InpSenkou", 52);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-EURUSD\EURUSD-Ichimoku.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResTen.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResKij.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
                metaResSpA.Add(splitResStr[3] != "" ? Convert.ToDouble(splitResStr[3]) : Double.NaN);
                metaResSpB.Add(splitResStr[4] != "" ? Convert.ToDouble(splitResStr[4]) : Double.NaN);
                metaResChk.Add(splitResStr[5] != "" ? Convert.ToDouble(splitResStr[5]) : Double.NaN);
                metaResSpA2.Add(splitResStr[6] != "" ? Convert.ToDouble(splitResStr[6]) : Double.NaN);
                metaResSpB2.Add(splitResStr[7] != "" ? Convert.ToDouble(splitResStr[7]) : Double.NaN);
            }

            int bidsLen = testResTen.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(metaResTen[testInd] - testResTen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResKij[testInd] - testResKij[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA[testInd] - testResSpA[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA2[testInd] - testResSpA2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB[testInd] - testResSpB[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB2[testInd] - testResSpB2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResChk[testInd] - testResChk[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            metaResTen = new List<double>();
            testResTen = new List<double>();
            metaResKij = new List<double>();
            testResKij = new List<double>();
            metaResSpA = new List<double>();
            testResSpA = new List<double>();
            metaResSpB = new List<double>();
            testResSpB = new List<double>();
            metaResChk = new List<double>();
            testResChk = new List<double>();
            metaResSpA2 = new List<double>();
            testResSpA2 = new List<double>();
            metaResSpB2 = new List<double>();
            testResSpB2 = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtTenkanBuffer", testResTen);
            writer.AddMapping("ExtKijunBuffer", testResKij);
            writer.AddMapping("ExtSpanA_Buffer", testResSpA);
            writer.AddMapping("ExtSpanB_Buffer", testResSpB);
            writer.AddMapping("ExtChikouBuffer", testResChk);
            writer.AddMapping("ExtSpanA2_Buffer", testResSpA2);
            writer.AddMapping("ExtSpanB2_Buffer", testResSpB2);

            builder = new IndicatorBuilder<Bar>(typeof(Ichimoku.Ichimoku), reader, writer);
            builder.SetParameter("InpTenkan", 9);
            builder.SetParameter("InpKijun", 26);
            builder.SetParameter("InpSenkou", 52);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-Ichimoku.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResTen.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResKij.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
                metaResSpA.Add(splitResStr[3] != "" ? Convert.ToDouble(splitResStr[3]) : Double.NaN);
                metaResSpB.Add(splitResStr[4] != "" ? Convert.ToDouble(splitResStr[4]) : Double.NaN);
                metaResChk.Add(splitResStr[5] != "" ? Convert.ToDouble(splitResStr[5]) : Double.NaN);
                metaResSpA2.Add(splitResStr[6] != "" ? Convert.ToDouble(splitResStr[6]) : Double.NaN);
                metaResSpB2.Add(splitResStr[7] != "" ? Convert.ToDouble(splitResStr[7]) : Double.NaN);
            }

            int bidsLen = testResTen.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(metaResTen[testInd] - testResTen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResKij[testInd] - testResKij[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA[testInd] - testResSpA[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA2[testInd] - testResSpA2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB[testInd] - testResSpB[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB2[testInd] - testResSpB2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResChk[testInd] - testResChk[testInd]));
            }
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            metaResTen = new List<double>();
            testResTen = new List<double>();
            metaResKij = new List<double>();
            testResKij = new List<double>();
            metaResSpA = new List<double>();
            testResSpA = new List<double>();
            metaResSpB = new List<double>();
            testResSpB = new List<double>();
            metaResChk = new List<double>();
            testResChk = new List<double>();
            metaResSpA2 = new List<double>();
            testResSpA2 = new List<double>();
            metaResSpB2 = new List<double>();
            testResSpB2 = new List<double>();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02_indicators-XAUUSD\XAUUSD-M1-bids.txt");

            reader = new DirectReader<Bar>(new TTQuoteFileReader(file));
            reader.AddMapping("Bars", b => b);

            writer = new DirectWriter<Bar>();
            writer.AddMapping("ExtTenkanBuffer", testResTen);
            writer.AddMapping("ExtKijunBuffer", testResKij);
            writer.AddMapping("ExtSpanA_Buffer", testResSpA);
            writer.AddMapping("ExtSpanB_Buffer", testResSpB);
            writer.AddMapping("ExtChikouBuffer", testResChk);
            writer.AddMapping("ExtSpanA2_Buffer", testResSpA2);
            writer.AddMapping("ExtSpanB2_Buffer", testResSpB2);

            builder = new IndicatorBuilder<Bar>(typeof(Ichimoku.Ichimoku), reader, writer);
            builder.SetParameter("InpTenkan", 9);
            builder.SetParameter("InpKijun", 26);
            builder.SetParameter("InpSenkou", 52);

            builder.ReadAllAndBuild();

            file = File.OpenText(@"..\..\..\IndicatorFiles\2015.11.02-2015.11.03_indicators-XAUUSD\XAUUSD-Ichimoku.txt");
            string resStr;
            while ((resStr = file.ReadLine()) != null)
            {
                string[] splitResStr = resStr.Split('\t');
                metaResTen.Add(splitResStr[1] != "" ? Convert.ToDouble(splitResStr[1]) : Double.NaN);
                metaResKij.Add(splitResStr[2] != "" ? Convert.ToDouble(splitResStr[2]) : Double.NaN);
                metaResSpA.Add(splitResStr[3] != "" ? Convert.ToDouble(splitResStr[3]) : Double.NaN);
                metaResSpB.Add(splitResStr[4] != "" ? Convert.ToDouble(splitResStr[4]) : Double.NaN);
                metaResChk.Add(splitResStr[5] != "" ? Convert.ToDouble(splitResStr[5]) : Double.NaN);
                metaResSpA2.Add(splitResStr[6] != "" ? Convert.ToDouble(splitResStr[6]) : Double.NaN);
                metaResSpB2.Add(splitResStr[7] != "" ? Convert.ToDouble(splitResStr[7]) : Double.NaN);
            }

            int bidsLen = testResTen.Count;
            for (int testInd = 50; testInd < bidsLen; testInd++)
            {

                AssertX.Greater(1e-10, Math.Abs(metaResTen[testInd] - testResTen[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResKij[testInd] - testResKij[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA[testInd] - testResSpA[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpA2[testInd] - testResSpA2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB[testInd] - testResSpB[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResSpB2[testInd] - testResSpB2[testInd]));
                AssertX.Greater(1e-10, Math.Abs(metaResChk[testInd] - testResChk[testInd]));
            }
        }

    }
}

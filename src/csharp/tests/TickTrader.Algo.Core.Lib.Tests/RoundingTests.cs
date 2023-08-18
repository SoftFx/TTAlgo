using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TickTrader.Algo.Core.Lib.Tests
{
    [TestClass]
    public class RoundingTests
    {
        [TestMethod]
        public void RealCase_ProfitCommissionRounding()
        {
            var digits = 5;
            var open = 1.12376; var close = 1.10501; var volume = 18000.0;
            // -337.5000000000008
            var profit = (close - open) * volume;
            AssertFloorByDigits(profit, digits, -337.5);
            var feeModifier = 0.002; var conversionRate = 1.12376;
            // 80.910720000000012
            var commission = feeModifier * volume * conversionRate * 2;
            AssertCeilByDigits(commission, digits, 80.91072);
        }

        [TestMethod]
        public void SimpleTest_RoundByDigits()
        {
            var digits = 0;
            AssertRoundByDigits(0.01, digits, 0);
            AssertRoundByDigits(1.25, digits, 1);
            AssertRoundByDigits(2.75, digits, 3);
            AssertRoundByDigits(3.99, digits, 4);
            AssertRoundByDigits(-0.01, digits, 0);
            AssertRoundByDigits(-1.25, digits, -1);
            AssertRoundByDigits(-2.75, digits, -3);
            AssertRoundByDigits(-3.99, digits, -4);

            digits = 2;
            AssertRoundByDigits(0.1101, digits, 0.11);
            AssertRoundByDigits(1.2225, digits, 1.22);
            AssertRoundByDigits(2.3375, digits, 2.34);
            AssertRoundByDigits(3.4499, digits, 3.45);
            AssertRoundByDigits(-0.1101, digits, -0.11);
            AssertRoundByDigits(-1.2225, digits, -1.22);
            AssertRoundByDigits(-2.3375, digits, -2.34);
            AssertRoundByDigits(-3.4499, digits, -3.45);

            digits = 5;
            AssertRoundByDigits(0.1111101, digits, 0.11111);
            AssertRoundByDigits(1.2222225, digits, 1.22222);
            AssertRoundByDigits(2.3333375, digits, 2.33334);
            AssertRoundByDigits(3.4444499, digits, 3.44445);
            AssertRoundByDigits(-0.1111101, digits, -0.11111);
            AssertRoundByDigits(-1.2222225, digits, -1.22222);
            AssertRoundByDigits(-2.3333375, digits, -2.33334);
            AssertRoundByDigits(-3.4444499, digits, -3.44445);
        }

        [TestMethod]
        public void SimpleTest_FloorByDigits()
        {
            var digits = 0;
            AssertFloorByDigits(0.01, digits, 0);
            AssertFloorByDigits(1.25, digits, 1);
            AssertFloorByDigits(2.75, digits, 2);
            AssertFloorByDigits(3.99, digits, 3);
            AssertFloorByDigits(-0.01, digits, -1);
            AssertFloorByDigits(-1.25, digits, -2);
            AssertFloorByDigits(-2.75, digits, -3);
            AssertFloorByDigits(-3.99, digits, -4);

            digits = 2;
            AssertFloorByDigits(0.1101, digits, 0.11);
            AssertFloorByDigits(1.2225, digits, 1.22);
            AssertFloorByDigits(2.3375, digits, 2.33);
            AssertFloorByDigits(3.4499, digits, 3.44);
            AssertFloorByDigits(-0.1101, digits, -0.12);
            AssertFloorByDigits(-1.2225, digits, -1.23);
            AssertFloorByDigits(-2.3375, digits, -2.34);
            AssertFloorByDigits(-3.4499, digits, -3.45);

            digits = 5;
            AssertFloorByDigits(0.1111101, digits, 0.11111);
            AssertFloorByDigits(1.2222225, digits, 1.22222);
            AssertFloorByDigits(2.3333375, digits, 2.33333);
            AssertFloorByDigits(3.4444499, digits, 3.44444);
            AssertFloorByDigits(-0.1111101, digits, -0.11112);
            AssertFloorByDigits(-1.2222225, digits, -1.22223);
            AssertFloorByDigits(-2.3333375, digits, -2.33334);
            AssertFloorByDigits(-3.4444499, digits, -3.44445);
        }

        [TestMethod]
        public void SimpleTest_CeilByDigits()
        {
            var digits = 0;
            AssertCeilByDigits(0.01, digits, 1);
            AssertCeilByDigits(1.25, digits, 2);
            AssertCeilByDigits(2.75, digits, 3);
            AssertCeilByDigits(3.99, digits, 4);
            AssertCeilByDigits(-0.01, digits, 0);
            AssertCeilByDigits(-1.25, digits, -1);
            AssertCeilByDigits(-2.75, digits, -2);
            AssertCeilByDigits(-3.99, digits, -3);

            digits = 2;
            AssertCeilByDigits(0.1101, digits, 0.12);
            AssertCeilByDigits(1.2225, digits, 1.23);
            AssertCeilByDigits(2.3375, digits, 2.34);
            AssertCeilByDigits(3.4499, digits, 3.45);
            AssertCeilByDigits(-0.1101, digits, -0.11);
            AssertCeilByDigits(-1.2225, digits, -1.22);
            AssertCeilByDigits(-2.3375, digits, -2.33);
            AssertCeilByDigits(-3.4499, digits, -3.44);

            digits = 5;
            AssertCeilByDigits(0.1111101, digits, 0.11112);
            AssertCeilByDigits(1.2222225, digits, 1.22223);
            AssertCeilByDigits(2.3333375, digits, 2.33334);
            AssertCeilByDigits(3.4444499, digits, 3.44445);
            AssertCeilByDigits(-0.1111101, digits, -0.11111);
            AssertCeilByDigits(-1.2222225, digits, -1.22222);
            AssertCeilByDigits(-2.3333375, digits, -2.33333);
            AssertCeilByDigits(-3.4444499, digits, -3.44444);
        }

        [TestMethod]
        public void EdgeTest_RoundByDigits()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored
            
            int digits = 0;
            double val = 1.5;
            AssertRoundByDigits(val - edgeEps, digits, 1);
            AssertRoundByDigits(val - errEps, digits, 2);
            AssertRoundByDigits(val, digits, 2);

            val = -1.5;
            AssertRoundByDigits(val, digits, -2);
            AssertRoundByDigits(val + errEps, digits, -2);
            AssertRoundByDigits(val + edgeEps, digits, -1);

            digits = 2;
            val = 2.225;
            AssertRoundByDigits(val - edgeEps, digits, 2.22);
            AssertRoundByDigits(val - errEps, digits, 2.23);
            AssertRoundByDigits(val, digits, 2.23);

            val = -2.225;
            AssertRoundByDigits(val, digits, -2.23);
            AssertRoundByDigits(val + errEps, digits, -2.23);
            AssertRoundByDigits(val + edgeEps, digits, -2.22);

            digits = 5;
            val = 3.333335;
            AssertRoundByDigits(val - edgeEps, digits, 3.33333);
            AssertRoundByDigits(val - errEps, digits, 3.33334);
            AssertRoundByDigits(val, digits, 3.33334);

            val = -3.333335;
            AssertRoundByDigits(val, digits, -3.33334);
            AssertRoundByDigits(val + errEps, digits, -3.33334);
            AssertRoundByDigits(val + edgeEps, digits, -3.33333);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            digits = 0;
            val = 1111.5;
            AssertRoundByDigits(val - edgeEps, digits, 1111);
            AssertRoundByDigits(val - errEps, digits, 1112);
            AssertRoundByDigits(val, digits, 1112);

            val = -1111.5;
            AssertRoundByDigits(val, digits, -1112);
            AssertRoundByDigits(val + errEps, digits, -1112);
            AssertRoundByDigits(val + edgeEps, digits, -1111);

            digits = 2;
            val = 2222.225;
            AssertRoundByDigits(val - edgeEps, digits, 2222.22);
            AssertRoundByDigits(val - errEps, digits, 2222.23);
            AssertRoundByDigits(val, digits, 2222.23);

            val = -2222.225;
            AssertRoundByDigits(val, digits, -2222.23);
            AssertRoundByDigits(val + errEps, digits, -2222.23);
            AssertRoundByDigits(val + edgeEps, digits, -2222.22);

            digits = 5;
            val = 3333.333335;
            AssertRoundByDigits(val - edgeEps, digits, 3333.33333);
            AssertRoundByDigits(val - errEps, digits, 3333.33334);
            AssertRoundByDigits(val, digits, 3333.33334);

            val = -3333.333335;
            AssertRoundByDigits(val, digits, -3333.33334);
            AssertRoundByDigits(val + errEps, digits, -3333.33334);
            AssertRoundByDigits(val + edgeEps, digits, -3333.33333);
        }

        [TestMethod]
        public void EdgeTest_FloorByDigits()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored

            int digits = 0;
            double val = 2;
            AssertFloorByDigits(val - edgeEps, digits, 1);
            AssertFloorByDigits(val - errEps, digits, 2);

            val = -2;
            AssertFloorByDigits(val - edgeEps, digits, -3);
            AssertFloorByDigits(val - errEps, digits, -2);

            digits = 2;
            val = 2.22;
            AssertFloorByDigits(val - edgeEps, digits, 2.21);
            AssertFloorByDigits(val - errEps, digits, 2.22);

            val = -2.22;
            AssertFloorByDigits(val - edgeEps, digits, -2.23);
            AssertFloorByDigits(val - errEps, digits, -2.22);

            digits = 5;
            val = 3.33333;
            AssertFloorByDigits(val - edgeEps, digits, 3.33332);
            AssertFloorByDigits(val - errEps, digits, 3.33333);

            val = -3.33333;
            AssertFloorByDigits(val - edgeEps, digits, -3.33334);
            AssertFloorByDigits(val - errEps, digits, -3.33333);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            digits = 0;
            val = 1112;
            AssertFloorByDigits(val - edgeEps, digits, 1111);
            AssertFloorByDigits(val - errEps, digits, 1112);

            val = -1112;
            AssertFloorByDigits(val - edgeEps, digits, -1113);
            AssertFloorByDigits(val - errEps, digits, -1112);

            digits = 2;
            val = 2222.22;
            AssertFloorByDigits(val - edgeEps, digits, 2222.21);
            AssertFloorByDigits(val - errEps, digits, 2222.22);

            val = -2222.22;
            AssertFloorByDigits(val - edgeEps, digits, -2222.23);
            AssertFloorByDigits(val - errEps, digits, -2222.22);

            digits = 5;
            val = 3333.33333;
            AssertFloorByDigits(val - edgeEps, digits, 3333.33332);
            AssertFloorByDigits(val - errEps, digits, 3333.33333);

            val = -3333.33333;
            AssertFloorByDigits(val - edgeEps, digits, -3333.33334);
            AssertFloorByDigits(val - errEps, digits, -3333.33333);
        }

        [TestMethod]
        public void EdgeTest_CeilByDigits()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored

            int digits = 0;
            double val = 2;
            AssertCeilByDigits(val + errEps, digits, 2);
            AssertCeilByDigits(val + edgeEps, digits, 3);

            val = -2;
            AssertCeilByDigits(val + errEps, digits, -2);
            AssertCeilByDigits(val + edgeEps, digits, -1);

            digits = 2;
            val = 2.22;
            AssertCeilByDigits(val + errEps, digits, 2.22);
            AssertCeilByDigits(val + edgeEps, digits, 2.23);

            val = -2.22;
            AssertCeilByDigits(val + errEps, digits, -2.22);
            AssertCeilByDigits(val + edgeEps, digits, -2.21);

            digits = 5;
            val = 3.33333;
            AssertCeilByDigits(val + errEps, digits, 3.33333);
            AssertCeilByDigits(val + edgeEps, digits, 3.33334);

            val = -3.33333;
            AssertCeilByDigits(val + errEps, digits, -3.33333);
            AssertCeilByDigits(val + edgeEps, digits, -3.33332);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            digits = 0;
            val = 1112;
            AssertCeilByDigits(val + errEps, digits, 1112);
            AssertCeilByDigits(val + edgeEps, digits, 1113);

            val = -1112;
            AssertCeilByDigits(val + errEps, digits, -1112);
            AssertCeilByDigits(val + edgeEps, digits, -1111);

            digits = 2;
            val = 2222.22;
            AssertCeilByDigits(val + errEps, digits, 2222.22);
            AssertCeilByDigits(val + edgeEps, digits, 2222.23);

            val = -2222.22;
            AssertCeilByDigits(val + errEps, digits, -2222.22);
            AssertCeilByDigits(val + edgeEps, digits, -2222.21);

            digits = 5;
            val = 3333.33333;
            AssertCeilByDigits(val + errEps, digits, 3333.33333);
            AssertCeilByDigits(val + edgeEps, digits, 3333.33334);

            val = -3333.33333;
            AssertCeilByDigits(val + errEps, digits, -3333.33333);
            AssertCeilByDigits(val + edgeEps, digits, -3333.33332);
        }

        [TestMethod]
        public void SimpleTest_RoundByStep()
        {
            var step = 1e-2;
            AssertRoundByStep(0.1101, step, 0.11);
            AssertRoundByStep(1.2225, step, 1.22);
            AssertRoundByStep(2.3375, step, 2.34);
            AssertRoundByStep(3.4499, step, 3.45);
            AssertRoundByStep(-0.1101, step, -0.11);
            AssertRoundByStep(-1.2225, step, -1.22);
            AssertRoundByStep(-2.3375, step, -2.34);
            AssertRoundByStep(-3.4499, step, -3.45);

            step = 1e-5;
            AssertRoundByStep(0.1111101, step, 0.11111);
            AssertRoundByStep(1.2222225, step, 1.22222);
            AssertRoundByStep(2.3333375, step, 2.33334);
            AssertRoundByStep(3.4444499, step, 3.44445);
            AssertRoundByStep(-0.1111101, step, -0.11111);
            AssertRoundByStep(-1.2222225, step, -1.22222);
            AssertRoundByStep(-2.3333375, step, -2.33334);
            AssertRoundByStep(-3.4444499, step, -3.44445);

            step = 5e-3;
            AssertRoundByStep(0.00501, step, 0.005);
            AssertRoundByStep(1.006, step, 1.005);
            AssertRoundByStep(2.008, step, 2.01);
            AssertRoundByStep(3.00999, step, 3.01);
            AssertRoundByStep(4.01001, step, 4.01);
            AssertRoundByStep(5.012, step, 5.01);
            AssertRoundByStep(6.0135, step, 6.015);
            AssertRoundByStep(7.01499, step, 7.015);
            AssertRoundByStep(-0.00501, step, -0.005);
            AssertRoundByStep(-1.006, step, -1.005);
            AssertRoundByStep(-2.008, step, -2.01);
            AssertRoundByStep(-3.00999, step, -3.01);
            AssertRoundByStep(-4.01001, step, -4.01);
            AssertRoundByStep(-5.012, step, -5.01);
            AssertRoundByStep(-6.0135, step, -6.015);
            AssertRoundByStep(-7.01499, step, -7.015);
        }

        [TestMethod]
        public void SimpleTest_FloorByStep()
        {
            var step = 1e-2;
            AssertFloorByStep(0.1101, step, 0.11);
            AssertFloorByStep(1.2225, step, 1.22);
            AssertFloorByStep(2.3375, step, 2.33);
            AssertFloorByStep(3.4499, step, 3.44);
            AssertFloorByStep(-0.1101, step, -0.12);
            AssertFloorByStep(-1.2225, step, -1.23);
            AssertFloorByStep(-2.3375, step, -2.34);
            AssertFloorByStep(-3.4499, step, -3.45);

            step = 1e-5;
            AssertFloorByStep(0.1111101, step, 0.11111);
            AssertFloorByStep(1.2222225, step, 1.22222);
            AssertFloorByStep(2.3333375, step, 2.33333);
            AssertFloorByStep(3.4444499, step, 3.44444);
            AssertFloorByStep(-0.1111101, step, -0.11112);
            AssertFloorByStep(-1.2222225, step, -1.22223);
            AssertFloorByStep(-2.3333375, step, -2.33334);
            AssertFloorByStep(-3.4444499, step, -3.44445);

            step = 5e-3;
            AssertFloorByStep(0.00501, step, 0.005);
            AssertFloorByStep(1.006, step, 1.005);
            AssertFloorByStep(2.008, step, 2.005);
            AssertFloorByStep(3.00999, step, 3.005);
            AssertFloorByStep(4.01001, step, 4.01);
            AssertFloorByStep(5.012, step, 5.01);
            AssertFloorByStep(6.0135, step, 6.01);
            AssertFloorByStep(7.01499, step, 7.01);
            AssertFloorByStep(-0.00501, step, -0.01);
            AssertFloorByStep(-1.006, step, -1.01);
            AssertFloorByStep(-2.008, step, -2.01);
            AssertFloorByStep(-3.00999, step, -3.01);
            AssertFloorByStep(-4.01001, step, -4.015);
            AssertFloorByStep(-5.012, step, -5.015);
            AssertFloorByStep(-6.0135, step, -6.015);
            AssertFloorByStep(-7.01499, step, -7.015);
        }

        [TestMethod]
        public void SimpleTest_CeilByStep()
        {
            var step = 1e-2;
            AssertCeilByStep(0.1101, step, 0.12);
            AssertCeilByStep(1.2225, step, 1.23);
            AssertCeilByStep(2.3375, step, 2.34);
            AssertCeilByStep(3.4499, step, 3.45);
            AssertCeilByStep(-0.1101, step, -0.11);
            AssertCeilByStep(-1.2225, step, -1.22);
            AssertCeilByStep(-2.3375, step, -2.33);
            AssertCeilByStep(-3.4499, step, -3.44);

            step = 1e-5;
            AssertCeilByStep(0.1111101, step, 0.11112);
            AssertCeilByStep(1.2222225, step, 1.22223);
            AssertCeilByStep(2.3333375, step, 2.33334);
            AssertCeilByStep(3.4444499, step, 3.44445);
            AssertCeilByStep(-0.1111101, step, -0.11111);
            AssertCeilByStep(-1.2222225, step, -1.22222);
            AssertCeilByStep(-2.3333375, step, -2.33333);
            AssertCeilByStep(-3.4444499, step, -3.44444);

            step = 5e-3;
            AssertCeilByStep(0.00501, step, 0.01);
            AssertCeilByStep(1.006, step, 1.01);
            AssertCeilByStep(2.008, step, 2.01);
            AssertCeilByStep(3.00999, step, 3.01);
            AssertCeilByStep(4.01001, step, 4.015);
            AssertCeilByStep(5.012, step, 5.015);
            AssertCeilByStep(6.0135, step, 6.015);
            AssertCeilByStep(7.01499, step, 7.015);
            AssertCeilByStep(-0.00501, step, -0.005);
            AssertCeilByStep(-1.006, step, -1.005);
            AssertCeilByStep(-2.008, step, -2.005);
            AssertCeilByStep(-3.00999, step, -3.005);
            AssertCeilByStep(-4.01001, step, -4.01);
            AssertCeilByStep(-5.012, step, -5.01);
            AssertCeilByStep(-6.0135, step, -6.01);
            AssertCeilByStep(-7.01499, step, -7.01);
        }

        [TestMethod]
        public void EdgeTest_RoundByStep()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored

            var step = 1e-2;
            var val = 2.225;
            AssertRoundByStep(val - edgeEps, step, 2.22);
            AssertRoundByStep(val - errEps, step, 2.23);
            AssertRoundByStep(val, step, 2.23);

            val = -2.225;
            AssertRoundByStep(val, step, -2.23);
            AssertRoundByStep(val + errEps, step, -2.23);
            AssertRoundByStep(val + edgeEps, step, -2.22);

            step = 1e-5;
            val = 3.333335;
            AssertRoundByStep(val - edgeEps, step, 3.33333);
            AssertRoundByStep(val - errEps, step, 3.33334);
            AssertRoundByStep(val, step, 3.33334);

            val = -3.333335;
            AssertRoundByStep(val, step, -3.33334);
            AssertRoundByStep(val + errEps, step, -3.33334);
            AssertRoundByStep(val + edgeEps, step, -3.33333);

            step = 5e-3;
            val = 4.2275;
            AssertRoundByStep(val - edgeEps, step, 4.225);
            AssertRoundByStep(val - errEps, step, 4.23);
            AssertRoundByStep(val, step, 4.23);

            val = -4.2275;
            AssertRoundByStep(val, step, -4.23);
            AssertRoundByStep(val + errEps, step, -4.23);
            AssertRoundByStep(val + edgeEps, step, -4.225);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            step = 1e-2;
            val = 2222.225;
            AssertRoundByStep(val - edgeEps, step, 2222.22);
            AssertRoundByStep(val - errEps, step, 2222.23);
            AssertRoundByStep(val, step, 2222.23);

            val = -2222.225;
            AssertRoundByStep(val, step, -2222.23);
            AssertRoundByStep(val + errEps, step, -2222.23);
            AssertRoundByStep(val + edgeEps, step, -2222.22);

            step = 1e-5;
            val = 3333.333335;
            AssertRoundByStep(val - edgeEps, step, 3333.33333);
            AssertRoundByStep(val - errEps, step, 3333.33334);
            AssertRoundByStep(val, step, 3333.33334);

            val = -3333.333335;
            AssertRoundByStep(val, step, -3333.33334);
            AssertRoundByStep(val + errEps, step, -3333.33334);
            AssertRoundByStep(val + edgeEps, step, -3333.33333);

            step = 5e-3;
            val = 4444.2275;
            AssertRoundByStep(val - edgeEps, step, 4444.225);
            AssertRoundByStep(val - errEps, step, 4444.23);
            AssertRoundByStep(val, step, 4444.23);

            val = -4444.2275;
            AssertRoundByStep(val, step, -4444.23);
            AssertRoundByStep(val + errEps, step, -4444.23);
            AssertRoundByStep(val + edgeEps, step, -4444.225);
        }

        [TestMethod]
        public void EdgeTest_FloorByStep()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored

            var step = 1e-2;
            var val = 2.22;
            AssertFloorByStep(val - edgeEps, step, 2.21);
            AssertFloorByStep(val - errEps, step, 2.22);

            val = -2.22;
            AssertFloorByStep(val - edgeEps, step, -2.23);
            AssertFloorByStep(val - errEps, step, -2.22);

            step = 1e-5;
            val = 3.33333;
            AssertFloorByStep(val - edgeEps, step, 3.33332);
            AssertFloorByStep(val - errEps, step, 3.33333);

            val = -3.33333;
            AssertFloorByStep(val - edgeEps, step, -3.33334);
            AssertFloorByStep(val - errEps, step, -3.33333);

            step = 5e-3;
            val = 4.225;
            AssertFloorByStep(val - edgeEps, step, 4.22);
            AssertFloorByStep(val - errEps, step, 4.225);

            val = -4.225;
            AssertFloorByStep(val - edgeEps, step, -4.23);
            AssertFloorByStep(val - errEps, step, -4.225);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            step = 1e-2;
            val = 2222.22;
            AssertFloorByStep(val - edgeEps, step, 2222.21);
            AssertFloorByStep(val - errEps, step, 2222.22);

            val = -2222.22;
            AssertFloorByStep(val - edgeEps, step, -2222.23);
            AssertFloorByStep(val - errEps, step, -2222.22);

            step = 1e-5;
            val = 3333.33333;
            AssertFloorByStep(val - edgeEps, step, 3333.33332);
            AssertFloorByStep(val - errEps, step, 3333.33333);

            val = -3333.33333;
            AssertFloorByStep(val - edgeEps, step, -3333.33334);
            AssertFloorByStep(val - errEps, step, -3333.33333);

            step = 5e-3;
            val = 4444.225;
            AssertFloorByStep(val - edgeEps, step, 4444.22);
            AssertFloorByStep(val - errEps, step, 4444.225);

            val = -4444.225;
            AssertFloorByStep(val - edgeEps, step, -4444.23);
            AssertFloorByStep(val - errEps, step, -4444.225);
        }

        [TestMethod]
        public void EdgeTest_CeilByStep()
        {
            double edgeEps = 1.1e-12; // still significant
            double errEps = 9e-13; // calc error which is ignored

            var step = 1e-2;
            var val = 2.22;
            AssertCeilByStep(val + errEps, step, 2.22);
            AssertCeilByStep(val + edgeEps, step, 2.23);

            val = -2.22;
            AssertCeilByStep(val + errEps, step, -2.22);
            AssertCeilByStep(val + edgeEps, step, -2.21);

            step = 1e-5;
            val = 3.33333;
            AssertCeilByStep(val + errEps, step, 3.33333);
            AssertCeilByStep(val + edgeEps, step, 3.33334);

            val = -3.33333;
            AssertCeilByStep(val + errEps, step, -3.33333);
            AssertCeilByStep(val + edgeEps, step, -3.33332);

            step = 5e-3;
            val = 4.225;
            AssertCeilByStep(val + errEps, step, 4.225);
            AssertCeilByStep(val + edgeEps, step, 4.23);

            val = -4.225;
            AssertCeilByStep(val + errEps, step, -4.225);
            AssertCeilByStep(val + edgeEps, step, -4.22);

            // With large integer part error should scale
            // values in range [1e3; 1e4)
            edgeEps = 1.1e-9;
            errEps = 9e-10;

            step = 1e-2;
            val = 2222.22;
            AssertCeilByStep(val + errEps, step, 2222.22);
            AssertCeilByStep(val + edgeEps, step, 2222.23);

            val = -2222.22;
            AssertCeilByStep(val + errEps, step, -2222.22);
            AssertCeilByStep(val + edgeEps, step, -2222.21);

            step = 1e-5;
            val = 3333.33333;
            AssertCeilByStep(val + errEps, step, 3333.33333);
            AssertCeilByStep(val + edgeEps, step, 3333.33334);

            val = -3333.33333;
            AssertCeilByStep(val + errEps, step, -3333.33333);
            AssertCeilByStep(val + edgeEps, step, -3333.33332);

            step = 5e-3;
            val = 4444.225;
            AssertCeilByStep(val + errEps, step, 4444.225);
            AssertCeilByStep(val + edgeEps, step, 4444.23);

            val = -4444.225;
            AssertCeilByStep(val + errEps, step, -4444.225);
            AssertCeilByStep(val + edgeEps, step, -4444.22);
        }

        [TestMethod]
        public void EdgeTest_SkipRounding()
        {
            // If total number of digits before and after decimal sepator > MaxDigits
            // Then rounding is skipped

            int digits = 5;
            double step = 1e-5;
            double val = 123456789.123456;
            // total precision = 8 + 5 > MaxDigits;
            AssertRoundByDigits(val, digits, val);
            AssertFloorByDigits(val, digits, val);
            AssertCeilByDigits(val, digits, val);
            AssertRoundByStep(val, step, val);
            AssertFloorByStep(val, step, val);
            AssertCeilByStep(val, step, val);

            val = 12345678.123456;
            // total precision = 7 + 5 == MaxDigits;
            AssertRoundByDigits(val, digits, 12345678.12346);
            AssertFloorByDigits(val, digits, 12345678.12345);
            AssertCeilByDigits(val, digits, 12345678.12346);
            AssertRoundByStep(val, step, 12345678.12346);
            AssertFloorByStep(val, step, 12345678.12345);
            AssertCeilByStep(val, step, 12345678.12346);

            val = 1234567.123456;
            // total precision = 6 + 5 < MaxDigits;
            AssertRoundByDigits(val, digits, 1234567.12346);
            AssertFloorByDigits(val, digits, 1234567.12345);
            AssertCeilByDigits(val, digits, 1234567.12346);
            AssertRoundByStep(val, step, 1234567.12346);
            AssertFloorByStep(val, step, 1234567.12345);
            AssertCeilByStep(val, step, 1234567.12346);

            digits = 3;
            step = 1e-3;
            val = 12345678900.1234;
            // total precision = 10 + 3 > MaxDigits;
            AssertRoundByDigits(val, digits, val);
            AssertFloorByDigits(val, digits, val);
            AssertCeilByDigits(val, digits, val);
            AssertRoundByStep(val, step, val);
            AssertFloorByStep(val, step, val);
            AssertCeilByStep(val, step, val);

            val = 1234567890.1234;
            // total precision = 9 + 3 == MaxDigits;
            AssertRoundByDigits(val, digits, 1234567890.123);
            AssertFloorByDigits(val, digits, 1234567890.123);
            AssertCeilByDigits(val, digits, 1234567890.124);
            AssertRoundByStep(val, step, 1234567890.123);
            AssertFloorByStep(val, step, 1234567890.123);
            AssertCeilByStep(val, step, 1234567890.124);

            val = 123456789.1234;
            // total precision = 8 + 3 < MaxDigits;
            AssertRoundByDigits(val, digits, 123456789.123);
            AssertFloorByDigits(val, digits, 123456789.123);
            AssertCeilByDigits(val, digits, 123456789.124);
            AssertRoundByStep(val, step, 123456789.123);
            AssertFloorByStep(val, step, 123456789.123);
            AssertCeilByStep(val, step, 123456789.124);
        }


        private static void AssertRoundByDigits(double val, int digits, double expectedRes)
            => Assert.AreEqual(expectedRes, val.RoundBy(digits));

        private static void AssertFloorByDigits(double val, int digits, double expectedRes)
            => Assert.AreEqual(expectedRes, val.FloorBy(digits));

        private static void AssertCeilByDigits(double val, int digits, double expectedRes)
            => Assert.AreEqual(expectedRes, val.CeilBy(digits));

        private static void AssertRoundByStep(double val, double step, double expectedRes)
            => Assert.AreEqual(expectedRes, val.RoundBy(step));

        private static void AssertFloorByStep(double val, double step, double expectedRes)
            => Assert.AreEqual(expectedRes, val.FloorBy(step));

        private static void AssertCeilByStep(double val, double step, double expectedRes)
            => Assert.AreEqual(expectedRes, val.CeilBy(step));
    }
}

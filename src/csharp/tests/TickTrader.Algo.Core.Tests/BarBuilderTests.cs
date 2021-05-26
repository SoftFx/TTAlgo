using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests
{
    [TestClass]
    public class BarBuilderTests
    {
        [TestMethod]
        public void BarVector_EmptyAppend()
        {
            var vector = new BarVector(Feed.Types.Timeframe.H1);

            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 11), 1.22, 0);

            Assert.AreEqual(1, vector.Count);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), vector[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), vector[0].CloseTime);
            Assert.AreEqual(1.22, vector[0].Open);
            Assert.AreEqual(1.22, vector[0].Close);
            Assert.AreEqual(1.22, vector[0].High);
            Assert.AreEqual(1.22, vector[0].Low);
        }

        [TestMethod]
        public void BarVector_AppendSameBar()
        {
            var vector = new BarVector(Feed.Types.Timeframe.H1);

            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 11), 1.22, 0);
            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 14), 1.21, 0);
            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 16), 1.24, 0);

            Assert.AreEqual(1, vector.Count);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), vector[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), vector[0].CloseTime);
            Assert.AreEqual(1.22, vector[0].Open);
            Assert.AreEqual(1.24, vector[0].Close);
            Assert.AreEqual(1.24, vector[0].High);
            Assert.AreEqual(1.21, vector[0].Low);
        }

        [TestMethod]
        public void BarVector_Append2Bars()
        {
            var vector = new BarVector(Feed.Types.Timeframe.H1);

            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 11), 1.22, 0);
            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 16, 46), 1.23, 0);
            vector.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 18, 22), 1.21, 0);

            Assert.AreEqual(2, vector.Count);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), vector[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), vector[0].CloseTime);
            Assert.AreEqual(1.22, vector[0].Open);
            Assert.AreEqual(1.22, vector[0].Close);
            Assert.AreEqual(1.22, vector[0].High);
            Assert.AreEqual(1.22, vector[0].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), vector[1].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), vector[1].CloseTime);
            Assert.AreEqual(1.23, vector[1].Open);
            Assert.AreEqual(1.21, vector[1].Close);
            Assert.AreEqual(1.23, vector[1].High);
            Assert.AreEqual(1.21, vector[1].Low);
        }

        [TestMethod]
        public void BarVector_MasterSlave_SlaveSkip()
        {
            var master = new BarVector(Feed.Types.Timeframe.H1);
            var slave = new BarVector(master);

            // master 14, 16
            // slave 14, 16

            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 11), 1.22, 0);
            slave.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 2, 48), 1.27, 0); // skipped
            slave.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 3, 2), 1.29, 0); // skipped
            slave.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 11, 35), 1.26, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 12, 23), 1.20, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 14, 28), 1.19, 0);

            Assert.AreEqual(2, master.Count);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), master[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), master[0].CloseTime);
            Assert.AreEqual(1.22, master[0].Open);
            Assert.AreEqual(1.22, master[0].Close);
            Assert.AreEqual(1.22, master[0].High);
            Assert.AreEqual(1.22, master[0].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), master[1].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), master[1].CloseTime);
            Assert.AreEqual(1.20, master[1].Open);
            Assert.AreEqual(1.19, master[1].Close);
            Assert.AreEqual(1.20, master[1].High);
            Assert.AreEqual(1.19, master[1].Low);

            Assert.AreEqual(2, slave.Count);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), slave[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), slave[0].CloseTime);
            Assert.AreEqual(double.NaN, slave[0].Open);
            Assert.AreEqual(double.NaN, slave[0].Close);
            Assert.AreEqual(double.NaN, slave[0].High);
            Assert.AreEqual(double.NaN, slave[0].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), slave[1].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), slave[1].CloseTime);
            Assert.AreEqual(1.26, slave[1].Open);
            Assert.AreEqual(1.26, slave[1].Close);
            Assert.AreEqual(1.26, slave[1].High);
            Assert.AreEqual(1.26, slave[1].Low);
        }

        [TestMethod]
        public void BarVector_MasterSlave_SlaveFill()
        {
            var master = new BarVector(Feed.Types.Timeframe.H1);
            var slave = new BarVector(master);

            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 13, 11), 1.23, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 44, 2), 1.21, 0);
            slave.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 47, 51), 2.6, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 05, 47), 1.29, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 07, 13), 1.31, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 16, 27), 1.20, 0);
            master.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 22, 08), 1.18, 0);
            slave.AppendQuote(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 34, 30), 2.4, 0);


            Assert.AreEqual(5, master.Count);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), master[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), master[0].CloseTime);
            Assert.AreEqual(1.23, master[0].Open);
            Assert.AreEqual(1.23, master[0].Close);
            Assert.AreEqual(1.23, master[0].High);
            Assert.AreEqual(1.23, master[0].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), master[1].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), master[1].CloseTime);
            Assert.AreEqual(1.21, master[1].Open);
            Assert.AreEqual(1.21, master[1].Close);
            Assert.AreEqual(1.21, master[1].High);
            Assert.AreEqual(1.21, master[1].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), master[2].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), master[2].CloseTime);
            Assert.AreEqual(1.29, master[2].Open);
            Assert.AreEqual(1.31, master[2].Close);
            Assert.AreEqual(1.31, master[2].High);
            Assert.AreEqual(1.29, master[2].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), master[3].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 00, 00), master[3].CloseTime);
            Assert.AreEqual(1.20, master[3].Open);
            Assert.AreEqual(1.20, master[3].Close);
            Assert.AreEqual(1.20, master[3].High);
            Assert.AreEqual(1.20, master[3].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 00, 00), master[4].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 19, 00, 00), master[4].CloseTime);
            Assert.AreEqual(1.18, master[4].Open);
            Assert.AreEqual(1.18, master[4].Close);
            Assert.AreEqual(1.18, master[4].High);
            Assert.AreEqual(1.18, master[4].Low);

            Assert.AreEqual(5, slave.Count);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 14, 00, 00), slave[0].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), slave[0].CloseTime);
            Assert.AreEqual(double.NaN, slave[0].Open);
            Assert.AreEqual(double.NaN, slave[0].Close);
            Assert.AreEqual(double.NaN, slave[0].High);
            Assert.AreEqual(double.NaN, slave[0].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 15, 00, 00), slave[1].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), slave[1].CloseTime);
            Assert.AreEqual(2.6, slave[1].Open);
            Assert.AreEqual(2.6, slave[1].Close);
            Assert.AreEqual(2.6, slave[1].High);
            Assert.AreEqual(2.6, slave[1].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 16, 00, 00), slave[2].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), slave[2].CloseTime);
            Assert.AreEqual(2.6, slave[2].Open);
            Assert.AreEqual(2.6, slave[2].Close);
            Assert.AreEqual(2.6, slave[2].High);
            Assert.AreEqual(2.6, slave[2].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 17, 00, 00), slave[3].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 00, 00), slave[3].CloseTime);
            Assert.AreEqual(2.6, slave[3].Open);
            Assert.AreEqual(2.6, slave[3].Close);
            Assert.AreEqual(2.6, slave[3].High);
            Assert.AreEqual(2.6, slave[3].Low);

            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 18, 00, 00), slave[4].OpenTime);
            Assert.AreEqual(TimestampHelper.FromDateAndTime(2015, 1, 1, 19, 00, 00), slave[4].CloseTime);
            Assert.AreEqual(2.4, slave[4].Open);
            Assert.AreEqual(2.4, slave[4].Close);
            Assert.AreEqual(2.4, slave[4].High);
            Assert.AreEqual(2.4, slave[4].Low);
        }
    }
}

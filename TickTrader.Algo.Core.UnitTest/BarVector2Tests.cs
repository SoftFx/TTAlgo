using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.UnitTest
{
    [TestClass]
    public class BarVector2Tests
    {
        const double NoVal = 0;

        [TestMethod]
        public void BarVector2_SlaveFirst()
        {
            var master = BarVector2.Create(Api.TimeFrames.D);
            var slave = BarVector2.Create(master);

            slave.AppendQuote(new DateTime(2017, 1, 1), 1.1, 0);
            slave.AppendQuote(new DateTime(2017, 1, 2), 1.2, 0);
            slave.AppendQuote(new DateTime(2017, 1, 4), 1.4, 0);

            master.AppendQuote(new DateTime(2017, 1, 1), 5.1, 0);
            master.AppendQuote(new DateTime(2017, 1, 3), 5.3, 0);
            master.AppendQuote(new DateTime(2017, 1, 4), 5.4, 0);
            master.AppendQuote(new DateTime(2017, 1, 5), 5.5, 0);

            Assert.AreEqual(4, master.Count);
            Assert.AreEqual(5.1, master[0].Open);
            Assert.AreEqual(5.3, master[1].Open);
            Assert.AreEqual(5.4, master[2].Open);
            Assert.AreEqual(5.5, master[3].Open);

            Assert.AreEqual(3, slave.Count);

            Assert.AreEqual(1.1, slave[0].Open);
            Assert.AreEqual(NoVal, slave[1].Open);
            Assert.AreEqual(1.4, slave[2].Open);
        }

        [TestMethod]
        public void BarVector2_MasterFirst_1()
        {
            var master = BarVector2.Create(Api.TimeFrames.D);
            var slave = BarVector2.Create(master);

            master.AppendQuote(new DateTime(2017, 1, 1), 5.1, 0);
            master.AppendQuote(new DateTime(2017, 1, 3), 5.3, 0);
            master.AppendQuote(new DateTime(2017, 1, 4), 5.4, 0); // +
            master.AppendQuote(new DateTime(2017, 1, 5), 5.5, 0);
            master.AppendQuote(new DateTime(2017, 1, 6), 5.6, 0); // +

            slave.AppendQuote(new DateTime(2016, 12, 27), 0.27, 0); // -
            slave.AppendQuote(new DateTime(2016, 12, 29), 0.29, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 2), 1.2, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 4), 1.4, 0); // +
            slave.AppendQuote(new DateTime(2017, 1, 6), 1.6, 0); // +


            Assert.AreEqual(5, master.Count);
            Assert.AreEqual(5.1, master[0].Open);
            Assert.AreEqual(5.3, master[1].Open);
            Assert.AreEqual(5.4, master[2].Open);
            Assert.AreEqual(5.5, master[3].Open);
            Assert.AreEqual(5.6, master[4].Open);

            Assert.AreEqual(5, slave.Count);

            Assert.AreEqual(NoVal, slave[0].Open);
            Assert.AreEqual(NoVal, slave[1].Open);
            Assert.AreEqual(1.4, slave[2].Open);
            Assert.AreEqual(NoVal, slave[3].Open);
            Assert.AreEqual(1.6, slave[4].Open);
        }

        [TestMethod]
        public void BarVector2_MasterFirst_2()
        {
            var master = BarVector2.Create(Api.TimeFrames.D);
            var slave = BarVector2.Create(master);

            master.AppendQuote(new DateTime(2017, 1, 1), 5.1, 0);
            master.AppendQuote(new DateTime(2017, 1, 3), 5.3, 0);
            master.AppendQuote(new DateTime(2017, 1, 4), 5.4, 0); // +
            master.AppendQuote(new DateTime(2017, 1, 5), 5.5, 0);
            master.AppendQuote(new DateTime(2017, 1, 6), 5.6, 0); // +

            slave.AppendQuote(new DateTime(2017, 1, 2), 1.2, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 4), 1.4, 0); // +
            slave.AppendQuote(new DateTime(2017, 1, 6), 1.6, 0); // +

            Assert.AreEqual(5, master.Count);
            Assert.AreEqual(5.1, master[0].Open);
            Assert.AreEqual(5.3, master[1].Open);
            Assert.AreEqual(5.4, master[2].Open);
            Assert.AreEqual(5.5, master[3].Open);
            Assert.AreEqual(5.6, master[4].Open);

            Assert.AreEqual(5, slave.Count);

            Assert.AreEqual(NoVal, slave[0].Open);
            Assert.AreEqual(NoVal, slave[1].Open);
            Assert.AreEqual(1.4, slave[2].Open);
            Assert.AreEqual(NoVal, slave[3].Open);
            Assert.AreEqual(1.6, slave[4].Open);
        }

        [TestMethod]
        public void BarVector2_MasterFirst_Exact()
        {
            var master = BarVector2.Create(Api.TimeFrames.D);
            var slave = BarVector2.Create(master);

            master.AppendQuote(new DateTime(2017, 1, 1), 5.1, 0);
            master.AppendQuote(new DateTime(2017, 1, 3), 5.3, 0);
            master.AppendQuote(new DateTime(2017, 1, 4), 5.4, 0); // +
            master.AppendQuote(new DateTime(2017, 1, 5), 5.5, 0);
            master.AppendQuote(new DateTime(2017, 1, 6), 5.6, 0); // +

            slave.AppendQuote(new DateTime(2017, 1, 1), 1.1, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 3), 1.3, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 4), 1.4, 0); // -
            slave.AppendQuote(new DateTime(2017, 1, 5), 1.5, 0); // +
            slave.AppendQuote(new DateTime(2017, 1, 6), 1.6, 0); // +

            Assert.AreEqual(5, master.Count);
            Assert.AreEqual(5.1, master[0].Open);
            Assert.AreEqual(5.3, master[1].Open);
            Assert.AreEqual(5.4, master[2].Open);
            Assert.AreEqual(5.5, master[3].Open);
            Assert.AreEqual(5.6, master[4].Open);

            Assert.AreEqual(5, slave.Count);

            Assert.AreEqual(1.1, slave[0].Open);
            Assert.AreEqual(1.3, slave[1].Open);
            Assert.AreEqual(1.4, slave[2].Open);
            Assert.AreEqual(1.5, slave[3].Open);
            Assert.AreEqual(1.6, slave[4].Open);
        }
    }
}

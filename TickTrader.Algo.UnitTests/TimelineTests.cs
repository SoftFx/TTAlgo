using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.BotTerminal.Model.Charts;

namespace TickTrader.Algo.UnitTests
{
    [TestClass]
    public class TimelineTests
    {
        [TestMethod]
        public void MathSampler_1Day_Test()
        {
            MathSampler sampler = new MathSampler(TimeSpan.FromDays(1));
            DateTime result = sampler.RoundUp(new DateTime(2015, 12, 11, 7, 27, 31, 112));
            Assert.AreEqual(new DateTime(2015, 12, 12, 0, 0, 0), result);
        }

        [TestMethod]
        public void MathSampler_15Minutes_Test()
        {
            MathSampler sampler = new MathSampler(TimeSpan.FromMinutes(15));
            DateTime result = sampler.RoundUp(new DateTime(2015, 12, 11, 7, 27, 31, 112));
            Assert.AreEqual(new DateTime(2015, 12, 11, 7, 30, 0), result);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;

namespace TickTrader.Algo.Calculator.Tests.MarginCalculations
{
    [TestClass]
    public class MarginRequestTests
    {
        private const double TestVolume = 5.31;

        [TestMethod]
        public void Limit_NoHidden()
        {
            var request = new MarginRequest(Domain.OrderInfo.Types.Type.Limit, false);

            Assert.AreEqual(request.Type, Domain.OrderInfo.Types.Type.Limit);
            Assert.AreEqual(request.Volume, 0.0);

            Assert.IsFalse(request.IsHiddenLimit);
        }

        [TestMethod]
        public void Limit_Hidden()
        {
            var request = new MarginRequest(Domain.OrderInfo.Types.Type.Limit, true);

            Assert.AreEqual(request.Type, Domain.OrderInfo.Types.Type.Limit);
            Assert.AreEqual(request.Volume, 0.0);

            Assert.IsTrue(request.IsHiddenLimit);
        }

        [TestMethod]
        public void Stop_NoHidden_Volume()
        {
            var request = new MarginRequest(TestVolume, Domain.OrderInfo.Types.Type.Stop, false);

            Assert.AreEqual(request.Type, Domain.OrderInfo.Types.Type.Stop);
            Assert.AreEqual(request.Volume, TestVolume);

            Assert.IsFalse(request.IsHiddenLimit);
        }

        [TestMethod]
        public void Stop_HiddenIgnore_Volume()
        {
            var request = new MarginRequest(TestVolume, Domain.OrderInfo.Types.Type.Stop, true);

            Assert.AreEqual(request.Type, Domain.OrderInfo.Types.Type.Stop);
            Assert.AreEqual(request.Volume, TestVolume);

            Assert.IsFalse(request.IsHiddenLimit);
        }

        [TestMethod]
        public void VolumeChange()
        {
            var request = new MarginRequest(TestVolume, Domain.OrderInfo.Types.Type.Limit, true);

            Assert.AreEqual(request.Type, Domain.OrderInfo.Types.Type.Limit);
            Assert.AreEqual(request.Volume, TestVolume);

            Assert.IsTrue(request.IsHiddenLimit);

            request = request.WithVolume(2 * TestVolume);

            Assert.AreEqual(request.Volume, 2 * TestVolume);
        }
    }
}

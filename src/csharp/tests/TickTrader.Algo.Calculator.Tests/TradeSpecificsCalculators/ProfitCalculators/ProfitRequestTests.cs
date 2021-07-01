using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator.Tests.ProfitCalculators
{
    [TestClass]
    public class ProfitRequestTests
    {
        [TestMethod]
        public void BuyRequest()
        {
            const double price = 2.1;
            const double volume = 1.9;
            const OrderInfo.Types.Side side = OrderInfo.Types.Side.Buy;

            var request = new ProfitRequest(price, volume, side);

            Assert.AreEqual(price, request.Price);
            Assert.AreEqual(volume, request.Volume);
            Assert.AreEqual(side, request.Side);
        }

        [TestMethod]
        public void SellRequest()
        {
            const double price = 2.2;
            const double volume = 0.3;
            const OrderInfo.Types.Side side = OrderInfo.Types.Side.Sell;

            var request = new ProfitRequest(price, volume, side);

            Assert.AreEqual(price, request.Price);
            Assert.AreEqual(volume, request.Volume);
            Assert.AreEqual(side, request.Side);
        }
    }
}

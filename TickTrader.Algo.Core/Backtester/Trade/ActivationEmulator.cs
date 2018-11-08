using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class ActivationEmulator
    {
        private static IEnumerable<ActivationRecord> NoActivatons = Enumerable.Empty<ActivationRecord>();

        private Dictionary<string, ActivationRegistry> indexes = new Dictionary<string, ActivationRegistry>();

        /// <summary>
        /// Register limit order or position for activation monitoring
        /// </summary>
        /// <param name="order"></param>
        /// <param name="acc"></param>
        public ActivationRecord AddOrder(OrderAccessor order, RateUpdate currentRate)
        {
            ActivationRegistry index;
            if (!indexes.TryGetValue(order.Symbol, out index))
            {
                index = new ActivationRegistry();
                indexes.Add(order.Symbol, index);
            }

            return index.AddOrder(order, currentRate);
        }

        public bool RemoveOrder(OrderAccessor order)
        {
            ActivationRegistry index;
            if (!indexes.TryGetValue(order.Symbol, out index))
                return false;

            return index.RemoveOrder(order);
        }

        public void ResetOrderActivation(OrderAccessor order)
        {
            ActivationRegistry index;
            if (!indexes.TryGetValue(order.Symbol, out index))
                return;

            index.ResetOrderActivation(order);
        }

        /// <summary>
        /// Checks all pending orders against specified symbol rate.
        /// All returned orders are automatically removed form the index.
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public IEnumerable<ActivationRecord> CheckPendingOrders(RateUpdate rate)
        {
            ActivationRegistry index;
            if (indexes.TryGetValue(rate.Symbol, out index))
                return index.CheckPendingOrders(rate);

            return NoActivatons;
        }
    }
}

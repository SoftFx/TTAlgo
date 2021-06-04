using System.Collections.Generic;
using TickTrader.Algo.Calculator;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class ActivationEmulator
    {
        //private static List<ActivationRecord> NoActivatons = Enumerable.Empty<ActivationRecord>();

        private readonly AlgoMarketState _state;

        private List<ActivationRecord> _result = new List<ActivationRecord>();

        public ActivationEmulator(AlgoMarketState state)
        {
            _state = state;
        }

        /// <summary>
        /// Register limit order or position for activation monitoring
        /// </summary>
        /// <param name="order"></param>
        /// <param name="acc"></param>
        public ActivationRecord AddOrder(OrderAccessor order, IRateInfo currentRate)
        {
            var node = _state.GetSymbolNodeOrNull(order.Info.Symbol);
            var index = node.ActivationIndex as ActivationRegistry;
            if (index == null)
                index = new ActivationRegistry();
            return index.AddOrder(order, currentRate);
        }

        public bool RemoveOrder(OrderAccessor order)
        {
            var node = _state.GetSymbolNodeOrNull(order.Info.Symbol);
            var index = node.ActivationIndex as ActivationRegistry;
            if (index == null)
                return false;

            return index.RemoveOrder(order);
        }

        //public void ResetOrderActivation(OrderAccessor order)
        //{
        //    ActivationRegistry index;
        //    if (!indexes.TryGetValue(order.Symbol, out index))
        //        return;

        //    index.ResetOrderActivation(order);
        //}

        /// <summary>
        /// Checks all pending orders against specified symbol rate.
        /// All returned orders are automatically removed form the index.
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public List<ActivationRecord> CheckPendingOrders(AlgoMarketNode node)
        {
            _result.Clear();
            var index = node.ActivationIndex as ActivationRegistry;
            index?.CheckPendingOrders(node.Rate, _result);
            return _result;
        }
    }
}

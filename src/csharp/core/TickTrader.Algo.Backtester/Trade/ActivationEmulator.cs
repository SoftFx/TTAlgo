using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class ActivationEmulator
    {
        //private static List<ActivationRecord> NoActivatons = Enumerable.Empty<ActivationRecord>();

        private readonly AlgoMarketState _state;
        private readonly Dictionary<string, ActivationRegistry> _activationIndex = new Dictionary<string, ActivationRegistry>();

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
            var symbol = order.Info.Symbol;
            if (!_activationIndex.TryGetValue(symbol, out var index))
            {
                index = new ActivationRegistry();
                _activationIndex.Add(symbol, index);
            }

            return index.AddOrder(order, currentRate);
        }

        public bool RemoveOrder(OrderAccessor order)
        {
            if (!_activationIndex.TryGetValue(order.Info.Symbol, out var index))
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
        public List<ActivationRecord> CheckPendingOrders(IRateInfo rate)
        {
            _result.Clear();
            if (_activationIndex.TryGetValue(rate.Symbol, out var index))
                index.CheckPendingOrders(rate, _result);
            return _result;
        }
    }
}

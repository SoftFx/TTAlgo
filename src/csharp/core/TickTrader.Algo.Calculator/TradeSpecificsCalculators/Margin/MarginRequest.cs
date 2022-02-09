using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    public sealed class MarginRequest : IMarginCalculateRequest
    {
        private readonly bool _isHidden;


        public OrderInfo.Types.Type Type { get; }

        public double Volume { get; private set; }

        public bool IsHiddenLimit => Type.IsLimit() && _isHidden;


        public MarginRequest(OrderInfo.Types.Type type, bool isHidden)
        {
            _isHidden = isHidden;

            Type = type;
        }

        public MarginRequest(double volume, OrderInfo.Types.Type type, bool isHidden) : this(type, isHidden)
        {
            Volume = volume;
        }

        public MarginRequest WithVolume(double volume)
        {
            Volume = volume;

            return this;
        }
    }
}

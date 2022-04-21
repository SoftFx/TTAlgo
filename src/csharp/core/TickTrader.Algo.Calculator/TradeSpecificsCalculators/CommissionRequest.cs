using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    public class CommissionRequest : ICommissionCalculateRequest
    {
        public OrderInfo.Types.Side Side { get; }

        public double Volume { get; }

        public bool IsReducedCommission { get; }

        public bool IsHiddenOrIceberg { get; }


        public CommissionRequest(OrderInfo.Types.Side side, double volume, bool isReducedCommission, bool isHiddenOrIceberg)
        {
            Side = side;
            Volume = volume;
            IsReducedCommission = isReducedCommission;
            IsHiddenOrIceberg = isHiddenOrIceberg;
        }
    }
}

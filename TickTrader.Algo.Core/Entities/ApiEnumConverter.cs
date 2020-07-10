using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class ApiEnumConverter
    {
        public static CommissionType Convert(Domain.CommissonInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.Type.PerUnit:
                    return CommissionType.PerUnit;
                case Domain.CommissonInfo.Types.Type.Percent:
                    return CommissionType.Percent;
                case Domain.CommissonInfo.Types.Type.Absolute:
                    return CommissionType.Absolute;
                case Domain.CommissonInfo.Types.Type.PercentageWaivedCash:
                    return CommissionType.PercentageWaivedCash;
                case Domain.CommissonInfo.Types.Type.PercentageWaivedEnhanced:
                    return CommissionType.PercentageWaivedEnhanced;
                case Domain.CommissonInfo.Types.Type.PerBond:
                    return CommissionType.PerBond;
                default:
                    throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static CommissionChargeType Convert(Domain.CommissonInfo.Types.ChargeType type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.ChargeType.PerTrade:
                    return CommissionChargeType.PerTrade;
                case Domain.CommissonInfo.Types.ChargeType.PerLot:
                    return CommissionChargeType.PerLot;
                default:
                    throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static CommissionChargeMethod Convert(Domain.CommissonInfo.Types.ChargeMethod method)
        {
            switch (method)
            {
                case Domain.CommissonInfo.Types.ChargeMethod.OneWay:
                    return CommissionChargeMethod.OneWay;
                case Domain.CommissonInfo.Types.ChargeMethod.RoundTurn:
                    return CommissionChargeMethod.RoundTurn;
                default:
                    throw new ArgumentException($"Unsupported commission charge method: {method}");
            }
        }
    }
}

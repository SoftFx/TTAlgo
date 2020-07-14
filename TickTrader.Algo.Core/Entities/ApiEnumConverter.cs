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
                case Domain.CommissonInfo.Types.Type.PerUnit: return CommissionType.PerUnit;
                case Domain.CommissonInfo.Types.Type.Percent: return CommissionType.Percent;
                case Domain.CommissonInfo.Types.Type.Absolute: return CommissionType.Absolute;
                case Domain.CommissonInfo.Types.Type.PercentageWaivedCash: return CommissionType.PercentageWaivedCash;
                case Domain.CommissonInfo.Types.Type.PercentageWaivedEnhanced: return CommissionType.PercentageWaivedEnhanced;
                case Domain.CommissonInfo.Types.Type.PerBond: return CommissionType.PerBond;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static CommissionChargeType Convert(Domain.CommissonInfo.Types.ChargeType type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.ChargeType.PerTrade: return CommissionChargeType.PerTrade;
                case Domain.CommissonInfo.Types.ChargeType.PerLot: return CommissionChargeType.PerLot;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static CommissionChargeMethod Convert(Domain.CommissonInfo.Types.ChargeMethod method)
        {
            switch (method)
            {
                case Domain.CommissonInfo.Types.ChargeMethod.OneWay: return CommissionChargeMethod.OneWay;
                case Domain.CommissonInfo.Types.ChargeMethod.RoundTurn: return CommissionChargeMethod.RoundTurn;

                default: throw new ArgumentException($"Unsupported commission charge method: {method}");
            }
        }

        public static AccountTypes Convert(Domain.AccountInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.AccountInfo.Types.Type.Gross: return AccountTypes.Gross;
                case Domain.AccountInfo.Types.Type.Net: return AccountTypes.Net;
                case Domain.AccountInfo.Types.Type.Cash: return AccountTypes.Cash;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static OrderSide ToApiEnum(this Domain.OrderInfo.Types.Side side)
        {
            switch (side)
            {
                case Domain.OrderInfo.Types.Side.Buy: return OrderSide.Buy;
                case Domain.OrderInfo.Types.Side.Sell: return OrderSide.Sell;

                default: throw new ArgumentException($"Unsupported type {side}");
            }
        }

        public static OrderType ToApiEnum(this Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Limit: return OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Market: return OrderType.Market;
                case Domain.OrderInfo.Types.Type.Stop: return OrderType.Stop;
                case Domain.OrderInfo.Types.Type.StopLimit: return OrderType.StopLimit;
                case Domain.OrderInfo.Types.Type.Position: return OrderType.Position;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }
    }
}

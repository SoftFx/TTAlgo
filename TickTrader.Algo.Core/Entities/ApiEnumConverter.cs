using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class ApiEnumConverter
    {
        public static CommissionType Convert(Domain.CommissonInfo.Types.ValueType type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.ValueType.Points: return CommissionType.PerUnit;
                case Domain.CommissonInfo.Types.ValueType.Percentage: return CommissionType.Percent;
                case Domain.CommissonInfo.Types.ValueType.Money: return CommissionType.Absolute;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
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

                default: throw new ArgumentException($"Unsupported order side {side}");
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

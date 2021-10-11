using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    public static class Extensions
    {
        public static void ThrowIfFailed(this OrderCmdResult result, TestOrderAction action)
        {
            if (result.IsFaulted)
                throw new Exception($"Failed to {action} order - {result.ResultCode}");
        }

        public static bool EI(this double a, double? b) => !b.HasValue || a.E(b.Value);

        public static bool GteI(this double a, double? b) => !b.HasValue || a.Gte(b.Value);

        public static bool LteI(this double a, double? b) => !b.HasValue || a.Lte(b.Value);

        public static bool IsServerError(this OrderCmdResultCodes code)
        {
            switch (code)
            {
                case OrderCmdResultCodes.OffQuotes:
                case OrderCmdResultCodes.DealerReject:
                case OrderCmdResultCodes.DealingTimeout:
                case OrderCmdResultCodes.ConnectionError:
                case OrderCmdResultCodes.OrderLocked:
                case OrderCmdResultCodes.ThrottlingError:
                case OrderCmdResultCodes.Timeout:
                    return true;

                default:
                    return false;
            }
        }
    }
}

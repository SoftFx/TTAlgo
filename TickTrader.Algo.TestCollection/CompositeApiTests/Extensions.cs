using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Auto.Tests
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
    }
}

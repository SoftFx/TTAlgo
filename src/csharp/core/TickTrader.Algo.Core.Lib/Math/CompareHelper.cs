using System;

namespace TickTrader.Algo.Core.Lib
{
    internal static class CompareHelper
    {
        private static readonly double[] EpsCache =
        {
            1e-12, // 0
            1e-11, // 1
            1e-10, // 2
            1e-9,  // 3
            1e-8,  // 4
            1e-7,  // 5
            1e-6,  // 6
            1e-5,  // 7
            1e-4,  // 8
            1e-4,  // 9
            1e-3,  // 10
            1e-3,  // 11
            1e-2,  // 12
            1e-2,  // 13
            1e-1,  // 14
        };


        public static bool E0(double a, double eps) => Math.Abs(a) < eps;

        public static bool E(double a, double b, double eps) => Math.Abs(a - b) < eps;

        public static bool Lt(double a, double b, double eps) => a < b - eps;

        public static bool Lte(double a, double b, double eps) => a < b + eps;

        public static bool Gt(double a, double b, double eps) => a > b + eps;

        public static bool Gte(double a, double b, double eps) => a > b - eps;

        public static double GetRecommendedEps(double d)
        {
            var index = GetCacheIndex(d);
            return index < EpsCache.Length ? EpsCache[index] : 1;
        }


        private static int GetCacheIndex(double d)
        {
            d = Math.Abs(d);
            if (d < 1e6)
            {
                if (d < 1e3)
                {
                    return d < 1e1 ? 0 : d < 1e2 ? 1 : 2;
                }
                else
                {
                    return d < 1e4 ? 3 : d < 1e5 ? 4 : 5;
                }
            }
            else if (d < 1e12)
            {
                if (d < 1e9)
                {
                    return d < 1e7 ? 6 : d < 1e8 ? 7 : 8;
                }
                else
                {
                    return d < 1e10 ? 9 : d < 1e11 ? 10 : 11;
                }
            }
            else
            {
                return d < 1e13 ? 12
                : d < 1e14 ? 13
                : d < 1e15 ? 14
                //: (int)Math.Log10(d);
                : 15; // extra not required
            }
        }
    }
}

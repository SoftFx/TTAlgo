namespace TickTrader.Algo.Core.Lib
{
    public static class CompareExtensions
    {
        private const double Eps = 1e-9;


        public static bool E0(this double a) => CompareHelper.E0(a, Eps);

        public static bool E(this double a, double b) => CompareHelper.E(a, b, Eps);

        public static bool Lt(this double a, double b) => CompareHelper.Lt(a, b, Eps);

        public static bool Lte(this double a, double b) => CompareHelper.Lte(a, b, Eps);

        public static bool Gt(this double a, double b) => CompareHelper.Gt(a, b, Eps);

        public static bool Gte(this double a, double b) => CompareHelper.Gte(a, b, Eps);


        public static bool E0(this double a, double eps) => CompareHelper.E0(a, eps);

        public static bool E(this double a, double b, double eps) => CompareHelper.E(a, b, eps);

        public static bool Lt(this double a, double b, double eps) => CompareHelper.Lt(a, b, eps);

        public static bool Lte(this double a, double b, double eps) => CompareHelper.Lte(a, b, eps);

        public static bool Gt(this double a, double b, double eps) => CompareHelper.Gt(a, b, eps);

        public static bool Gte(this double a, double b, double eps) => CompareHelper.Gte(a, b, eps);
    }
}

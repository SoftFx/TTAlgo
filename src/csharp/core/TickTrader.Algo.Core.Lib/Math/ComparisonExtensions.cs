namespace TickTrader.Algo.Core.Lib.Math
{
    public static class ComparisonExtensions
    {
        public const double Epsilon = 1e-9;


        /// <summary>
        /// Determines if a in (b - Epsilon; b + Epsilon). Equivalent of a equal to b
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool E(this double a, double b)
        {
            return a > b - Epsilon && a < b + Epsilon;
        }

        /// <summary>
        /// Determines if a in (-Inf; b - Epsilon). Equivalent of a lower than b
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool Lt(this double a, double b)
        {
            return a < b - Epsilon;
        }

        /// <summary>
        /// Determines if a in (-Inf; b + Epsilon). Equivalent of a lower than or equal to b
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool Lte(this double a, double b)
        {
            return a < b + Epsilon;
        }

        /// <summary>
        /// Determines if a in (b + Epsilon; Inf). Equivalent of a greater than b
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool Gt(this double a, double b)
        {
            return a > b + Epsilon;
        }

        /// <summary>
        /// Determines if a in (b - Epsilon; Inf). Equivalent of a greater than or equal to b
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool Gte(this double a, double b)
        {
            return a > b - Epsilon;
        }

        /// <summary>
        /// Determines if a in (-Epsilon; Epsilon). Equivalent of a equal to 0.0
        /// </summary>
        /// <returns>true if a is in expected range, false otherwise</returns>
        public static bool IsZero(this double a)
        {
            return a < Epsilon && a > -Epsilon;
        }
    }
}
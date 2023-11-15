namespace TickTrader.Algo.Core.Lib
{
    public static class RoundingExtensions
    {
        public static double RoundBy(this double val, int digits) => RoundingHelper.Round(val, digits);

        public static double FloorBy(this double val, int digits) => RoundingHelper.Floor(val, digits);

        public static double CeilBy(this double val, int digits) => RoundingHelper.Ceil(val, digits);

        public static double RoundBy(this double val, double step) => RoundingHelper.Round(val, step);

        public static double FloorBy(this double val, double step) => RoundingHelper.Floor(val, step);

        public static double CeilBy(this double val, double step) => RoundingHelper.Ceil(val, step);


        public static double? RoundBy(this double? val, int digits) => val?.RoundBy(digits);

        public static double? FloorBy(this double? val, int digits) => val?.FloorBy(digits);

        public static double? CeilBy(this double? val, int digits) => val?.CeilBy(digits);

        public static double? RoundBy(this double? val, double step) => val?.RoundBy(step);

        public static double? FloorBy(this double? val, double step) => val?.FloorBy(step);

        public static double? CeilBy(this double? val, double step) => val?.CeilBy(step);
    }
}

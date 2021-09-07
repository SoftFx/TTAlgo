namespace TickTrader.Algo.Core.Lib.Math
{
    public static class DoubleExtensions
    {
        public static double Filtration(this double? number)
        {
            return number is null || double.IsInfinity(number.Value) || double.IsNaN(number.Value) ? 0.0 : number.Value;
        }

        public static double Filtration(this double number)
        {
            return double.IsInfinity(number) || double.IsNaN(number) ? 0.0 : number;
        }
    }
}

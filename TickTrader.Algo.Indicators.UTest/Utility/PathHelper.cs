namespace TickTrader.Algo.Indicators.UTest.Utility
{
    public static class PathHelper
    {
        public static string MeasuresDir(string category, string indicatorName)
        {
            return $@"..\..\TestsData\{category}Tests\{indicatorName}\";
        }
    }
}

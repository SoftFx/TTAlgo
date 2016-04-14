namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Utility
{
    public static class PathHelper
    {
        public static string MeasuresOneDayDir(string symbol)
        {
            return $@"..\..\TestsData\LegacyTests\2015.11.02_indicators-{symbol}\";
        }

        public static string MeasuresTwoDayDir(string symbol)
        {
            return $@"..\..\TestsData\LegacyTests\2015.11.02-2015.11.03_indicators-{symbol}\";
        }

        public static string QuotesPath(string dir, string symbol)
        {
            return $"{dir}{symbol}-M1-bids.txt";
        }

        public static string AnswerPath(string dir, string symbol, string prefix)
        {
            return $"{dir}{symbol}-{prefix}.txt";
        }
    }
}

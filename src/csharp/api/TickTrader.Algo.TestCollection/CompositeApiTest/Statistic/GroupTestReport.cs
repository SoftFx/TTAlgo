namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct GroupTestReport
    {
        private static int _groupNumber;


        public string GroupInfo { get; }

        public string ErrorReport { get; }

        public GroupTestStatistic GroupStats { get; }

        public bool HasError => GroupStats.FailedCount > 0;


        internal GroupTestReport(string name, string report, GroupTestStatistic stats)
        {
            _groupNumber++;

            GroupInfo = name;
            ErrorReport = report;
            GroupStats = stats;
        }


        internal static void ResetStaticFields() => _groupNumber = 0;

        public override string ToString()
        {
            return $"Group #{_groupNumber}: {GroupInfo}\n{ErrorReport}";
        }
    }
}

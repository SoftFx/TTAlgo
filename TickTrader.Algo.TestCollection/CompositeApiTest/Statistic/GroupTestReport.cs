namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct GroupTestReport
    {
        public string GroupInfo { get; }

        public string ErrorReport { get; }

        public GroupTestStatistic GroupStats { get; }

        public bool HasError => GroupStats.FailedCount > 0;


        internal GroupTestReport(string name, string report, GroupTestStatistic stats)
        {
            GroupInfo = name;
            ErrorReport = report;
            GroupStats = stats;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

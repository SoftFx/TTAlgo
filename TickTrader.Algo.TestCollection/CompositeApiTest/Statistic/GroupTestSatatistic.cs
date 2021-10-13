namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct GroupTestStatistic
    {
        public long TotalMilliseconds { get; }

        public int TestCount { get; }

        public int FailedCount { get; }

        public int SuccCount => TestCount - FailedCount;

        public double PercentSucc => (double)SuccCount / TestCount * 100.0;


        public GroupTestStatistic(int testCount, int failed, long milliseconds)
        {
            TotalMilliseconds = milliseconds;
            TestCount = testCount;
            FailedCount = failed;
        }

        public static GroupTestStatistic operator +(GroupTestStatistic first, GroupTestStatistic second)
        {
            return new GroupTestStatistic(
                first.TestCount + second.TestCount,
                first.FailedCount + second.FailedCount,
                first.TotalMilliseconds + second.TotalMilliseconds);
        }

        public override string ToString()
        {
            return $"Tests count: {TestCount}, Failed: {FailedCount}, Succ: {PercentSucc:F3}%, Time: {TotalMilliseconds / 1000:F3}s";
        }
    }
}

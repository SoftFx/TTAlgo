namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct GroupTestResult
    {
        public static GroupTestResult Empty { get; } = new GroupTestResult(0, 0, 0);


        public long TotalMilliseconds { get; }

        public int TestCount { get; }

        public int FailedCount { get; }

        public int SuccCount => TestCount - FailedCount;

        public double PercentSucc => SuccCount / TestCount * 100;


        public GroupTestResult(int testCount, int failed, long milliseconds)
        {
            TotalMilliseconds = milliseconds;
            TestCount = testCount;
            FailedCount = failed;
        }

        public static GroupTestResult operator +(GroupTestResult first, GroupTestResult second)
        {
            return new GroupTestResult(
                first.TestCount + second.TestCount,
                first.FailedCount + second.FailedCount,
                first.TotalMilliseconds + second.TotalMilliseconds);
        }

        public override string ToString()
        {
            return $"Tests count: {TestCount}, Failed: {FailedCount}, Succ: {PercentSucc}%, Time: {TotalMilliseconds / 1000:F3}s";
        }
    }
}

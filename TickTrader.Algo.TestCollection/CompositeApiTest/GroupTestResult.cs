namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct GroupTestResult
    {
        public int TotalMilliseconds { get; }

        public int TestCount { get; }

        public int ErrorCount { get; }

        public int SuccCount => TestCount - ErrorCount;


        public GroupTestResult(int milliseconds, int testCount, int errors)
        {
            TotalMilliseconds = milliseconds;
            TestCount = testCount;
            ErrorCount = errors;
        }
    }
}

namespace TickTrader.SeriesStorage.Tests.Mocks
{
    internal class MockItem
    {
        public MockItem(int id, string val)
        {
            Id = id;
            Value = val;
        }

        public int Id { get; }
        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}

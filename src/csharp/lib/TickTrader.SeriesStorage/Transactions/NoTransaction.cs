namespace TickTrader.SeriesStorage
{
    public class NoTransaction : ITransaction
    {
        public static NoTransaction Instance { get; } = new NoTransaction();

        public void Abort()
        {
        }

        public void Commit()
        {
        }

        public void Dispose()
        {
        }
    }
}

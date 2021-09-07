using LightningDB;

namespace TickTrader.SeriesStorage.Lmdb
{
    internal class LmdbTransaction : ITransaction
    {
        public LmdbTransaction(LightningEnvironment env, bool readOnly)
        {
            Handle = env.BeginTransaction(readOnly ? TransactionBeginFlags.ReadOnly : TransactionBeginFlags.None);

            //System.Diagnostics.Debug.WriteLine("tr start " + Handle.Handle() + " thread=" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        public LightningTransaction Handle { get; }

        public void Abort()
        {
            Handle.Abort();
        }

        public void Commit()
        {
            Handle.Commit();
        }

        public void Dispose()
        {
            Handle.Dispose();

            //System.Diagnostics.Debug.WriteLine("tr end " + Handle.Handle() + " thread=" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

    }
}

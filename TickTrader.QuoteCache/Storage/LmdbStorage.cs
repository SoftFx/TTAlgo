using LightningDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.QuoteCache.Storage
{
    public class LmdbStorage
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        static LmdbStorage()
        {
            if (Environment.Is64BitProcess)
                LoadLibrary("x64\\lmdb.dll");
            else
                LoadLibrary("x86\\lmdb.dll");
        }

        public LmdbStorage()
        {
            //using (LightningEnvironment env = new LightningEnvironment("FeedCache2"))
            //{
            //    env.MaxDatabases = 2;
            //    env.Open();

            //    using (var tx = env.BeginTransaction())
            //    {
            //        using (var db = tx.OpenDatabase("hello.db", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            //        {
            //            tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
            //            tx.Commit();
            //        }
            //    }
            //}
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

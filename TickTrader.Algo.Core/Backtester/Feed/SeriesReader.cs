using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public abstract class SeriesReader
    {
        public RateUpdate Current { get; protected set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract bool MoveNext();
        public abstract SeriesReader Clone();
    }
}

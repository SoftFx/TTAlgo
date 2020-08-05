using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public abstract class SeriesReader
    {
        public IRateInfo Current { get; protected set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract bool MoveNext();
        public abstract SeriesReader Clone();
    }
}

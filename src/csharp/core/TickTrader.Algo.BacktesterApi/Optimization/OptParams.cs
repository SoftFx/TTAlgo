using System.Collections.Generic;

namespace TickTrader.Algo.BacktesterApi
{
    public class OptParams
    {
        public long Id { get; } = -1;

        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();


        public OptParams(long id)
        {
            Id = id;
        }
    }
}

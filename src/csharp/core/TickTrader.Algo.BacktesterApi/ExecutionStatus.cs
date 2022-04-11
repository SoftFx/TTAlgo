using System.Collections.Generic;

namespace TickTrader.Algo.BacktesterApi
{
    public class ExecutionStatus
    {
        public bool HasError { get; set; }

        public string Status { get; set; }

        public List<string> ErrorDetails { get; set; }
    }
}

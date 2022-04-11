using System.Collections.Generic;

namespace TickTrader.Algo.BacktesterApi
{
    public class ExecutionStatus
    {
        public static ExecutionStatus NotFound { get; } = new ExecutionStatus { Status = "Fatal error", HasError = true, ErrorDetails = new List<string> { "Execution status not found" } };


        public bool HasError { get; set; }

        public string Status { get; set; }

        public List<string> ErrorDetails { get; set; }


        public ExecutionStatus()
        {
            ErrorDetails = new List<string>();
        }
    }
}

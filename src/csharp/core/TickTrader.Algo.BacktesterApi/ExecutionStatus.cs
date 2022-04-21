using System;
using System.Collections.Generic;

namespace TickTrader.Algo.BacktesterApi
{
    public class ExecutionStatus
    {
        public static ExecutionStatus NotFound { get; } = new ExecutionStatus { HasError = true, Details = new List<string> { "Fatal error", "Execution status not found" }, ResultsNotCorrupted = false };


        public bool HasError { get; set; }

        public List<string> Details { get; set; }

        public bool ResultsNotCorrupted { get; set; }


        public ExecutionStatus()
        {
            Details = new List<string>();
        }


        public override string ToString()
        {
            return string.Join(Environment.NewLine, Details);
        }


        public void AddStatus(string status)
        {
            Details.Add(status);
        }

        public void SetError(string error, string errorDetails = null)
        {
            HasError = true;

            Details.Add(error);
            if (!string.IsNullOrEmpty(errorDetails))
                Details.Add(errorDetails);
        }
    }
}

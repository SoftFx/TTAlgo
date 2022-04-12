using System.Collections.Generic;
using System.Text;

namespace TickTrader.Algo.BacktesterApi
{
    public class ExecutionStatus
    {
        public static ExecutionStatus NotFound { get; } = new ExecutionStatus { Status = "Fatal error", HasError = true, ErrorDetails = new List<string> { "Execution status not found" }, ResultsNotCorrupted = false };


        public bool HasError { get; set; }

        public string Status { get; set; }

        public List<string> ErrorDetails { get; set; }

        public bool ResultsNotCorrupted { get; set; }


        public ExecutionStatus()
        {
            ErrorDetails = new List<string>();
        }


        public override string ToString()
        {
            if (!HasError)
                return Status;

            var sb = new StringBuilder();
            sb.Append(Status);
            foreach (var error in ErrorDetails)
            {
                sb.AppendLine();
                sb.Append(error);
            }
            return sb.ToString();
        }


        public void SetError(string error, string errorDetails = null)
        {
            if (string.IsNullOrEmpty(errorDetails))
                errorDetails = error;

            HasError = true;
            ErrorDetails.Add(errorDetails);
            if (string.IsNullOrEmpty(Status))
                Status = error;
        }
    }
}

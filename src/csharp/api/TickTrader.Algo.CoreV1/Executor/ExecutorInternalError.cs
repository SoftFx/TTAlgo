using System;

namespace TickTrader.Algo.CoreV1
{
    public class ExecutorInternalError
    {
        public Exception Exception { get; }

        public bool IsFatal { get; }


        public ExecutorInternalError(Exception exception, bool isFatal)
        {
            Exception = exception;
            IsFatal = isFatal;
        }
    }
}

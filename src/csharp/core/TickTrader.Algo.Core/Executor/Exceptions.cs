using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class ExecutorCoreException : Exception
    {
        public ExecutorCoreException(string message) : base(message)
        {
        }
    }

    public class ExecutorException : AlgoException
    {
        public ExecutorException(string msg) : base(msg)
        {
        }
    }

    public class AlgoOperationCanceledException : AlgoException
    {
        public AlgoOperationCanceledException(string msg) : base(msg)
        {
        }
    }

    public class NotEnoughDataException : AlgoException
    {
        public NotEnoughDataException(string msg) : base(msg)
        {
        }
    }
}

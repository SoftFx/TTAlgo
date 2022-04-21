using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class ExecutorException : AlgoException
    {
        public ExecutorException(string msg) : base(msg)
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

using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class MisconfigException : AlgoException
    {
        public MisconfigException(string message) : base(message)
        {
        }
    }

    public class StopOutException : AlgoException
    {
        public StopOutException(string message) : base(message)
        {
        }
    }

    public class MarginNotCalculatedException : AlgoException
    {
        public MarginNotCalculatedException(string message) : base(message)
        {

        }
    }

    public class NotEnoughMoneyException : AlgoException
    {
        public NotEnoughMoneyException(string message) : base(message)
        {

        }
    }

    public class MarketConfigurationException : AlgoException
    {
        public MarketConfigurationException(string message) : base(message)
        {

        }
    }

    public class SymbolNotFoundException : AlgoException
    {
        public SymbolNotFoundException(string message) : base(message) { }
    }
}

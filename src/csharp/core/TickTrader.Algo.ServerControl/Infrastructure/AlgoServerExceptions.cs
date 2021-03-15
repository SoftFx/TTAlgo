using System;

namespace TickTrader.Algo.ServerControl
{
    public class AlgoException : Exception
    {
        public AlgoException(string message) : base(message) { }
    }


    public class UnauthorizedException : AlgoException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class UnsupportedException : AlgoException
    {
        public UnsupportedException(string message) : base(message)
        {
        }
    }

    public class ConnectionFailedException : AlgoException
    {
        public ConnectionFailedException(string message) : base(message)
        {
        }
    }

    public class TimeoutException : AlgoException
    {
        public TimeoutException(string message) : base(message)
        {
        }
    }
}

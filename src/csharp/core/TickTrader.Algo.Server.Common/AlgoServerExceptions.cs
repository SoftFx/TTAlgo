using System;

namespace TickTrader.Algo.Server.Common
{
    public class AlgoServerException : Exception
    {
        public AlgoServerException(string message) : base(message) { }
    }


    public class UnauthorizedException : AlgoServerException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class UnsupportedException : AlgoServerException
    {
        public UnsupportedException(string message) : base(message)
        {
        }
    }

    public class ConnectionFailedException : AlgoServerException
    {
        public ConnectionFailedException(string message) : base(message)
        {
        }
    }

    public class TimeoutException : AlgoServerException
    {
        public TimeoutException(string message) : base(message)
        {
        }
    }
}

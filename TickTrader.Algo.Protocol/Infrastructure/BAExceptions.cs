using System;

namespace TickTrader.Algo.Protocol
{
    public class BAException : Exception
    {
        public BAException(string message) : base(message) { }
    }


    public class UnauthorizedException : BAException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class UnsupportedException : BAException
    {
        public UnsupportedException(string message) : base(message)
        {
        }
    }

    public class ConnectionFailedException : BAException
    {
        public ConnectionFailedException(string message) : base(message)
        {
        }
    }

    public class TimeoutException : BAException
    {
        public TimeoutException(string message) : base(message)
        {
        }
    }
}

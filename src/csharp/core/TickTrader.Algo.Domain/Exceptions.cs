using System;

namespace TickTrader.Algo.Domain
{
    public class AlgoException : Exception
    {
        public AlgoException(string message) : base(message)
        {
        }
    }

    public class AlgoPluginException : Exception
    {
        private readonly ExecutorErrorMsg _error;

        public AlgoPluginException(ExecutorErrorMsg error) : base()
        {
            _error = error;
        }

        public override string Message => _error.Message;

        public override string StackTrace => _error.Stacktrace;

        public override string ToString()
        {
            return $"{_error.ExceptionType}: {_error.Message}{Environment.NewLine}{_error.Stacktrace}";
        }
    }
}

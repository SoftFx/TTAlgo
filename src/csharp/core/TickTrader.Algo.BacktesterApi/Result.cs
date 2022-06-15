using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.BacktesterApi
{
    public class Result
    {
        private readonly Exception _ex;


        public static Result Ok { get; } = new Result();


        public string ErrorMessage { get; }

        public Exception Exception => _ex ?? (HasError ? new AlgoException(ErrorMessage) : null);

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);


        protected Result() { }

        public Result(string error)
        {
            ErrorMessage = error;
        }

        public Result(Exception error)
        {
            _ex = error;

            ErrorMessage = error.Message;
        }


        public static implicit operator bool(Result result)
        {
            return !result.HasError;
        }
    }


    public class Result<T> : Result
    {
        public T ResultValue { get; private set; }


        protected Result(T value) : base()
        {
            ResultValue = value;
        }

        public Result(string error) : base(error) { }

        public Result(Exception error) : base(error) { }


        public static new Result<T> Ok(T value) => new Result<T>(value);
    }
}

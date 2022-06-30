using System;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    public class CalculateResponseBase<T> : ICalculateResponse<T>
    {
        public T Value { get; protected set; }

        public CalculationError Error { get; protected set; }


        public bool IsCompleted => Error == CalculationError.None;

        public bool IsFailed => !IsCompleted;


        internal CalculateResponseBase(T value)
        {
            Value = value;
            Error = CalculationError.None;
        }

        internal CalculateResponseBase(T value, CalculationError error)
        {
            Value = value;
            Error = error;
        }


        public static implicit operator bool(CalculateResponseBase<T> response)
        {
            return response.IsCompleted;
        }
    }


    public sealed class CalculateResponse : CalculateResponseBase<double>
    {
        public static CalculateResponse OffQuotes { get; } = new CalculateResponse(CalculationError.OffQuote);

        public static CalculateResponse OffCrossQuotes { get; } = new CalculateResponse(CalculationError.OffCrossQuote);

        public static CalculateResponse NoCrossSymbol { get; } = new CalculateResponse(CalculationError.NoCrossSymbol);

        public static CalculateResponse SymbolNotFound { get; } = new CalculateResponse(CalculationError.SymbolNotFound);


        internal CalculateResponse(double value) : base(value)
        {

        }

        internal CalculateResponse(double value, CalculationError error) : base(value)
        {
            Value = error == CalculationError.None ? value : 0.0;
            Error = error;
        }

        private CalculateResponse(CalculationError error) : base(0.0)
        {
            Error = error;
        }
    }


    internal static class ResponseFactory
    {
        public static CalculateResponse Build(CalculationError error) => Build(0.0, error);

        public static CalculateResponse Build(double value, CalculationError error)
        {
            switch (error)
            {
                case CalculationError.None:
                    return new CalculateResponse(value);

                case CalculationError.OffQuote:
                    return CalculateResponse.OffQuotes;

                case CalculationError.NoCrossSymbol:
                    return CalculateResponse.NoCrossSymbol;

                case CalculationError.OffCrossQuote:
                    return CalculateResponse.OffCrossQuotes;

                case CalculationError.SymbolNotFound:
                    return CalculateResponse.SymbolNotFound;

                default:
                    throw new ArgumentException($"Unsupported type: {error}");
            }
        }
    }
}

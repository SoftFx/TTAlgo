namespace TickTrader.Algo.Domain.CalculatorInterfaces
{
    public enum CalculationError
    {
        None = 0,
        OffQuote,
        OffCrossQuote,
        SymbolNotFound,
        NoCrossSymbol,
    }

    public interface ISymbolCalculator
    {
        SymbolInfo SymbolInfo { get; }

        IMarginCalculator Margin { get; }

        IProfitCalculator Profit { get; }
    }

    public interface IMarginCalculator
    {
        ICalculateResponse<double> Calculate(IMarginCalculateRequest request);
    }

    public interface IProfitCalculator
    {
        ICalculateResponse<double> Calculate(IProfitCalculateRequest request);
    }

    public interface ICalculateResponse<T>
    {
        T Value { get; }

        bool IsCompleted { get; }

        bool IsFailed { get; }

        CalculationError Error { get; }
    }

    public interface IMarginCalculateRequest
    {
        OrderInfo.Types.Type Type { get; }

        double Volume { get; }

        bool IsHiddenLimit { get; }
    }

    public interface IProfitCalculateRequest
    {
        double Price { get; }

        OrderInfo.Types.Side Side { get; }

        double Volume { get; }
    }
}

using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.TradeSpecificsCalculators;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Conversions
{
    internal interface IOperation
    {
        ISideNode Operand { get; }

        CalculateResponse Calculate(double value);
    }


    internal abstract class BaseOperator
    {
        public ISideNode Operand { get; }


        protected BaseOperator() { }

        internal BaseOperator(ISideNode operand) => Operand = operand;


        protected virtual CalculateResponse Calculate(double? value)
        {
            if (Operand == null || Operand.Error == CalculationError.SymbolNotFound)
                return CalculateResponse.NoCrossSymbol;

            if (!Operand.HasValue)
                return CalculateResponse.OffCrossQuotes;

            return new CalculateResponse(value.Filtration());
        }
    }


    internal sealed class InvOperator : BaseOperator, IOperation
    {
        public CalculateResponse Calculate(double value) => new CalculateResponse(value.E(0.0) ? 0.0 : 1.0 / value);
    }


    internal sealed class MulOperator : BaseOperator, IOperation
    {
        internal MulOperator(ISideNode operand) : base(operand) { }


        public CalculateResponse Calculate(double value) => Calculate(value * Operand?.Value);
    }


    internal sealed class DivOperator : BaseOperator, IOperation
    {
        internal DivOperator(ISideNode operand) : base(operand) { }


        public CalculateResponse Calculate(double value) => Calculate(value / Operand?.Value);
    }
}

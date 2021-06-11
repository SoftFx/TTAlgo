using TickTrader.Algo.Core.Lib.Math;

namespace TickTrader.Algo.Calculator.Operators
{
    internal interface IOperation
    {
        ISideNode Operand { get; }

        double Calculate(double value);
    }

    internal sealed class InvOperator : IOperation
    {
        public ISideNode Operand { get; }

        public double Calculate(double value) => value.E(0.0) ? 0.0 : 1.0 / value;
    }

    internal sealed class MulOperator : IOperation
    {
        public ISideNode Operand { get; }

        internal MulOperator(ISideNode operand) => Operand = operand;

        public double Calculate(double value) => (value * Operand?.Value).Filtration();
    }

    internal sealed class DivOperator : IOperation
    {
        public ISideNode Operand { get; }

        internal DivOperator(ISideNode operand) => Operand = operand;

        public double Calculate(double value) => (value / Operand?.Value).Filtration();
    }
}

using System;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Conversions
{
    internal static class Formula
    {
        public static FormulaStub Direct { get; } = new FormulaStub(1, CalculationError.None);

        public static FormulaStub ErrorBuild { get; } = new FormulaStub(0, CalculationError.NoCrossSymbol);


        public static FormulaBuilder Get(ISideNode tracker) => new FormulaBuilder(tracker);

        public static FormulaBuilder Inv(ISideNode tracker) => new FormulaBuilder(tracker).Inv();
    }


    public interface IConversionFormula : IDisposable
    {
        double Value { get; }

        CalculationError Error { get; }
    }


    internal sealed class FormulaStub : IConversionFormula
    {
        public double Value { get; }

        public CalculationError Error { get; }


        internal FormulaStub(double value, CalculationError error)
        {
            Value = value;
            Error = error;
        }

        public void Dispose() { }
    }
}

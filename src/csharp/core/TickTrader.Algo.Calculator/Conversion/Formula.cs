namespace TickTrader.Algo.Calculator
{
    internal static class Formula
    {
        public static FormulaStub Direct { get; } = new FormulaStub(1, CalcErrorCodes.None);

        public static FormulaStub ErrorBuild { get; } = new FormulaStub(0, CalcErrorCodes.NoCrossSymbol);


        public static FormulaBuilder Get(ISideNode tracker) => new FormulaBuilder(tracker);

        public static FormulaBuilder Inv(ISideNode tracker) => new FormulaBuilder(tracker).Inv();
    }

    internal interface IConversionFormula
    {
        double Value { get; }

        CalcErrorCodes ErrorCode { get; }
    }

    internal sealed class FormulaStub : IConversionFormula
    {
        public double Value { get; }

        public CalcErrorCodes ErrorCode { get; }


        internal FormulaStub(double value, CalcErrorCodes error)
        {
            Value = value;
            ErrorCode = error;
        }
    }
}

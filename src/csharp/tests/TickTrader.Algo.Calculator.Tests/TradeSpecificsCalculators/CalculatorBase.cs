using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Tests.TradeSpecificsCalculators
{
    public abstract class CalculatorBase : AlgoMarketEmulator
    {
        protected string X, Y, Z, C;
        protected int _leverage = 1;

        protected Func<double> _conversionRate;

        protected SymbolInfo MainSymbol => Symbol[X + Y];

        protected ISymbolCalculator OrderCalculator => _algoMarket.GetCalculator(MainSymbol);


        protected abstract ICalculateResponse<double> ActualValue { get; }

        protected abstract Func<double> ExpectedValue { get; }


        public virtual void TestInit()
        {
            _leverage = 1;
            _conversionRate = () => 1.0;

            X = Y = Z = C = string.Empty;

            CreateAlgoMarket();
        }

        protected void InitEnviromentState(params string[] loadSymbols)
        {
            _account.BalanceCurrencyName = Z;
            _account.Leverage = _leverage;

            InitAlgoMarket(loadSymbols);
        }

        protected void IsSuccessfulTest()
        {
            var result = ActualValue;

            Assert.IsTrue(result.IsCompleted);
            Assert.AreEqual(CalculationError.None, result.Error);
            Assert.AreEqual(ExpectedValue(), result.Value);
        }

        protected void IsFailedTestCrossSymbolNotFound()
        {
            var result = ActualValue;

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(CalculationError.NoCrossSymbol, result.Error);
            Assert.AreEqual(0.0, result.Value);
        }

        protected void IsFailedTestOffQuotes()
        {
            var result = ActualValue;

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(CalculationError.OffQuote, result.Error);
            Assert.AreEqual(0.0, result.Value);
        }

        protected void IsFailedTestOffCrossQuotes()
        {
            var result = ActualValue;

            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(CalculationError.OffCrossQuote, result.Error);
            Assert.AreEqual(0.0, result.Value);
        }
    }
}

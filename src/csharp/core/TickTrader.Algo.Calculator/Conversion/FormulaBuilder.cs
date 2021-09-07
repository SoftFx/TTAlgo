using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Conversions
{
    internal sealed class FormulaBuilder : IConversionFormula
    {
        private readonly List<IOperation> _operations;
        private readonly ISideNode _node;

        private double _value;


        public double Value
        {
            get => Error == CalculationError.None ? _value : 0.0;

            private set => _value = value;
        }

        public CalculationError Error { get; private set; }


        internal FormulaBuilder(ISideNode node)
        {
            _operations = new List<IOperation>(2);

            _node = Subscribe(node);

            RecalculateFormula();
        }


        public FormulaBuilder Inv() => AddOperation(new InvOperator());

        public FormulaBuilder Mul(ISideNode operand) => AddOperation(new MulOperator(Subscribe(operand)));

        public FormulaBuilder Div(ISideNode operand) => AddOperation(new DivOperator(Subscribe(operand)));


        private FormulaBuilder AddOperation(IOperation operation)
        {
            _operations.Add(operation);

            RecalculateFormula();

            return this;
        }

        private void RecalculateFormula()
        {
            Error = _node?.Error ?? CalculationError.SymbolNotFound;

            if (Error == CalculationError.OffQuote && !IsDirectlyFormula)
                Error = CalculationError.OffCrossQuote;
            else
            if (Error == CalculationError.None)
            {
                Error = CalculationError.None;
                Value = _node.Value.Filtration();

                foreach (var operation in _operations)
                {
                    var result = operation.Calculate(Value);

                    if (!result)
                    {
                        Error = result.Error;
                        break;
                    }

                    Value = result.Value;
                }
            }
        }

        private ISideNode Subscribe(ISideNode node)
        {
            if (node != null)
                node.ValueUpdate += RecalculateFormula;

            return node;
        }

        private bool IsDirectlyFormula => _operations.Count == 0 || (_operations.Count == 1 && (_operations[0] is InvOperator));
    }
}

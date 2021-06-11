using System;
using System.Collections.Generic;
using TickTrader.Algo.Calculator.Operators;
using TickTrader.Algo.Core.Lib.Math;

namespace TickTrader.Algo.Calculator
{
    internal sealed class FormulaBuilder : IConversionFormula
    {
        private readonly List<IOperation> _operations;
        private readonly ISideNode _node;

        public double Value { get; private set; }

        public CalcErrorCodes ErrorCode => throw new NotImplementedException();

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
            Value = _node?.Value.Filtration() ?? 0.0;

            foreach (var operation in _operations)
                Value = operation.Calculate(Value);
        }

        private ISideNode Subscribe(ISideNode node)
        {
            if (node != null)
                node.ValueUpdate += RecalculateFormula;

            return node;
        }
    }
}

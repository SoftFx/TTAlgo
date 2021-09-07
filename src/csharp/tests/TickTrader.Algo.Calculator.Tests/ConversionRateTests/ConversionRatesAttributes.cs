using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TickTrader.Algo.Calculator.Tests
{
    public class CrossMarginCurrencyCategoryAttribute : TestCategoryBaseAttribute
    {
        private readonly List<string> _categorias = new() { "Margin Currency" };

        public override IList<string> TestCategories => _categorias;
    }

    public class CrossProfitCurrencyCategoryAttribute : TestCategoryBaseAttribute
    {
        private readonly List<string> _categorias = new() { "Profit Currency" };

        public override IList<string> TestCategories => _categorias;
    }

    public class DirectlyConvertionCategoryAttribute : TestCategoryBaseAttribute
    {
        private readonly List<string> _categorias = new() { "Directly" };

        public override IList<string> TestCategories => _categorias;
    }
}
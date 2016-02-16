using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal abstract class ChartNavigator
    {
        public ChartNavigator()
        {
            TimeAxis = CreateAxis();
        }

        public abstract IRange VisibleRange { get; }
        public AxisBase TimeAxis { get; private set; }

        protected abstract AxisBase CreateAxis();
    }

    internal class UniformChartNavigator : ChartNavigator
    {
        private IndexRange visibleRange = new IndexRange();

        public override IRange VisibleRange { get { return visibleRange; } }

        protected override AxisBase CreateAxis()
        {
            return new CategoryDateTimeAxis() { VisibleRange = visibleRange };
        }
    }

    internal class NonUniformChartNavigator : ChartNavigator
    {
        private DateRange visibleRange = new DateRange();

        public override IRange VisibleRange { get { return visibleRange; } }

        protected override AxisBase CreateAxis()
        {
            return new DateTimeAxis() { VisibleRange = visibleRange };
        }
    }
}

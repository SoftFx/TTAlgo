using Caliburn.Micro;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal abstract class ChartNavigator : PropertyChangedBase
    {
        private IRange visiableRange;

        public ChartNavigator()
        {
        }

        public IRange VisibleRange
        {
            get { return visiableRange; }
            set
            {
                visiableRange = value;
                NotifyOfPropertyChange("VisibleRange");
            }
        }

        public AxisBase CreateAxis()
        {
            var axis = CreateAxisInternal();
            Binding rangeBinding = new Binding("VisibleRange");
            rangeBinding.Source = this;
            rangeBinding.Mode = BindingMode.TwoWay;
            axis.SetBinding(AxisBase.VisibleRangeProperty, rangeBinding);
            return axis;
        }

        protected abstract AxisBase CreateAxisInternal();
    }

    internal class UniformChartNavigator : ChartNavigator
    {
        private IndexRange visibleRange = new IndexRange();

        protected override AxisBase CreateAxisInternal()
        {
            return new  CategoryDateTimeAxis();
        }
    }

    internal class NonUniformChartNavigator : ChartNavigator
    {
        private DateRange visibleRange = new DateRange();

        protected override AxisBase CreateAxisInternal()
        {
            return new DateTimeAxis() { VisibleRange = visibleRange };
        }
    }
}

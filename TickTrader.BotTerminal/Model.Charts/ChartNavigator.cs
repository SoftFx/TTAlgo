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

        public abstract void Update(int itemsCount, DateTime firstItemX, DateTime lastItemX);

        protected abstract AxisBase CreateAxisInternal();
    }

    internal class UniformChartNavigator : ChartNavigator
    {
        protected override AxisBase CreateAxisInternal()
        {
            return new CategoryDateTimeAxis();
        }

        public override void Update(int itemsCount, DateTime firstItemX, DateTime lastItemX)
        {
            int pageStart = itemsCount - 100;

            if (pageStart < 0)
                pageStart = 0;

            this.VisibleRange = new IndexRange(pageStart, itemsCount);
        }
    }

    internal class NonUniformChartNavigator : ChartNavigator
    {
        protected override AxisBase CreateAxisInternal()
        {
            return new DateTimeAxis();
        }

        public override void Update(int itemsCount, DateTime firstItemX, DateTime lastItemX)
        {
        }
    }
}

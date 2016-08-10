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

        public abstract void Init(int itemsCount, DateTime start, DateTime end);
        public abstract void Extend(int newCount, DateTime newEnd);

        protected abstract AxisBase CreateAxisInternal();
    }

    internal class UniformChartNavigator : ChartNavigator
    {
        private int defPageSize = 100;
        private int itemsCount;

        protected override AxisBase CreateAxisInternal()
        {
            return new CategoryDateTimeAxis();
        }

        public override void Init(int itemsCount, DateTime start, DateTime end)
        {
            this.itemsCount = itemsCount;
            int pageStart = itemsCount - defPageSize - 1;
            this.VisibleRange = new IndexRange(pageStart, itemsCount - 1);
        }

        public override void Extend(int newCount, DateTime newEnd)
        {
            var range = VisibleRange as IndexRange;
            if (range != null)
            {
                bool watchingEnd = itemsCount == 0 || range.Max == this.itemsCount - 1;

                this.itemsCount = newCount;

                if (watchingEnd)
                {
                    int newStart = itemsCount - range.Diff;
                    VisibleRange = new IndexRange(newStart, itemsCount - 1);
                }
            }
        }
    }

    internal class RealTimeChartNavigator : ChartNavigator
    {
        private TimeSpan defPageSize = TimeSpan.FromMinutes(2);
        private DateTime start;
        private DateTime end;

        protected override AxisBase CreateAxisInternal()
        {
            return new DateTimeAxis();
        }

        public override void Init(int itemsCount, DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
            var pageStart = end - defPageSize;
            this.VisibleRange = new DateRange(pageStart, end);
        }

        public override void Extend(int newCount, DateTime newEnd)
        {
            if (newEnd <= end)
                return;

            var range = VisibleRange as DateRange;
            if (range != null)
            {
                bool watchingEnd = range.Max >= end;

                this.end = newEnd;

                if (watchingEnd)
                {
                    var diff = range.Max - range.Min;
                    var pageStart = end - diff;
                    VisibleRange = new DateRange(pageStart, end);
                }
            }
        }
    }
}

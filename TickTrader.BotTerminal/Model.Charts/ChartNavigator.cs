using Caliburn.Micro;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.Axes.LabelProviders;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal abstract class ChartNavigator : PropertyChangedBase
    {
        private IRange visiableRange;
        //private IRange visiableRangeLimit;

        public ChartNavigator()
        {
        }

        public IRange VisibleRange
        {
            get { return visiableRange; }
            set
            {
                if (visiableRange == null || !visiableRange.Equals(value))
                {
                    //System.Diagnostics.Debug.WriteLine("VisibleRange changed!");
                    visiableRange = value;
                    NotifyOfPropertyChange("VisibleRange");
                }
            }
        }

        //public IRange VisibleRangeLimit
        //{
        //    get { return visiableRangeLimit; }
        //    set
        //    {
        //        if (visiableRangeLimit == null || !visiableRangeLimit.Equals(value))
        //        {
        //            System.Diagnostics.Debug.WriteLine("visiableRangeLimit changed!");
        //            visiableRangeLimit = value;
        //            NotifyOfPropertyChange("VisibleRangeLimit");
        //        }
        //    }
        //}

        public AxisBase CreateAxis()
        {
            var axis = CreateAxisInternal();

            Binding rangeBinding = new Binding("VisibleRange");
            rangeBinding.Source = this;
            rangeBinding.Mode = BindingMode.TwoWay;
            axis.SetBinding(AxisBase.VisibleRangeProperty, rangeBinding);
            
            //Binding rangeLimitBinding = new Binding("VisibleRangeLimit");
            //rangeLimitBinding.Source = this;
            //rangeLimitBinding.Mode = BindingMode.OneWay;
            //axis.SetBinding(AxisBase.VisibleRangeLimitProperty, rangeLimitBinding);

            axis.DrawMajorBands = false;
            return axis;
        }

        //public abstract void Init(int itemsCount, DateTime start, DateTime end);
        //public abstract void Extend(int newCount, DateTime newEnd);
        public abstract void OnDataLoaded(int itemsCount, DateTime start, DateTime end);

        protected abstract AxisBase CreateAxisInternal();
    }

    internal class UniformChartNavigator : ChartNavigator
    {
        //private int defPageSize = 100;
        //private int itemsCount;

        protected override AxisBase CreateAxisInternal()
        {
            return new CategoryDateTimeAxis()
            {
                AutoRange = AutoRange.Never,
                FontSize = 10,
                AutoTicks = false,
                MajorDelta = 2,
                MinorDelta = 1,
                IsLabelCullingEnabled = false
            };
        }

        public override void OnDataLoaded(int itemsCount, DateTime start, DateTime end)
        {
            //VisibleRange = null;

            if (itemsCount > 0)
                VisibleRange = new IndexRange(0, itemsCount + 500);
        }

        //public override void Init(int itemsCount, DateTime start, DateTime end)
        //{
        //    this.itemsCount = itemsCount;

        //    if (itemsCount < defPageSize)
        //        this.VisibleRange = new IndexRange(0, itemsCount - 1);
        //    else
        //    {
        //        int pageStart = itemsCount - defPageSize - 1;

        //        //if (pageStart < 0)
        //        //    VisibleRangeLimit = new IndexRange(pageStart, itemsCount - 1);
        //        //else
        //        //    VisibleRangeLimit = new IndexRange(0, itemsCount - 1);

        //        this.VisibleRange = new IndexRange(pageStart, itemsCount - 1);
        //    }
        //}

        //public override void Extend(int newCount, DateTime newEnd)
        //{
        //    //var rangeLimit = VisibleRangeLimit as IndexRange;
        //    //if (rangeLimit != null)
        //    //    VisibleRangeLimit = new IndexRange(rangeLimit.Min, newCount - 1);

        //    var range = VisibleRange as IndexRange;
        //    if (range != null)
        //    {
        //        bool watchingEnd = itemsCount == 0 || range.Max == this.itemsCount - 1;

        //        this.itemsCount = newCount;

        //        if (watchingEnd)
        //        {
        //            int min = itemsCount - range.Diff - 1;
        //            int max = itemsCount - 1;
        //            if (range.Min != min || range.Max != max)
        //                VisibleRange = new IndexRange(min, itemsCount - 1);
        //        }
        //    }
        //}
    }

    internal class RealTimeChartNavigator : ChartNavigator
    {
        //private TimeSpan defPageSize = TimeSpan.FromMinutes(2);
        //private DateTime start;
        //private DateTime end;

        protected override AxisBase CreateAxisInternal()
        {
            return new DateTimeAxis();
        }

        public override void OnDataLoaded(int itemsCount, DateTime start, DateTime end)
        {
        }

        //public override void Init(int itemsCount, DateTime start, DateTime end)
        //{
        //    this.start = start;
        //    this.end = end;
        //    var pageStart = end - defPageSize;
        //    this.VisibleRange = new DateRange(pageStart, end);
        //}

        //public override void Extend(int newCount, DateTime newEnd)
        //{
        //    if (newEnd <= end)
        //        return;

        //    var range = VisibleRange as DateRange;
        //    if (range != null)
        //    {
        //        bool watchingEnd = range.Max >= end;

        //        this.end = newEnd;

        //        if (watchingEnd)
        //        {
        //            var diff = range.Max - range.Min;
        //            var pageStart = end - diff;
        //            VisibleRange = new DateRange(pageStart, end);
        //        }
        //    }
        //}
    }
}

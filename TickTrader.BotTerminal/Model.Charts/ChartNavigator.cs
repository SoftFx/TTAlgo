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
                    System.Diagnostics.Debug.WriteLine("VisibleRange changed!");
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
            return new CategoryDateTimeAxis() { AutoRange = AutoRange.Never};
        }

        public override void Init(int itemsCount, DateTime start, DateTime end)
        {
            this.itemsCount = itemsCount;

            if (itemsCount < defPageSize)
                this.VisibleRange = new IndexRange(0, itemsCount - 1);
            else
            {
                int pageStart = itemsCount - defPageSize - 1;

                //if (pageStart < 0)
                //    VisibleRangeLimit = new IndexRange(pageStart, itemsCount - 1);
                //else
                //    VisibleRangeLimit = new IndexRange(0, itemsCount - 1);

                this.VisibleRange = new IndexRange(pageStart, itemsCount - 1);
            }
        }

        public override void Extend(int newCount, DateTime newEnd)
        {
            //var rangeLimit = VisibleRangeLimit as IndexRange;
            //if (rangeLimit != null)
            //    VisibleRangeLimit = new IndexRange(rangeLimit.Min, newCount - 1);

            var range = VisibleRange as IndexRange;
            if (range != null)
            {
                bool watchingEnd = itemsCount == 0 || range.Max == this.itemsCount - 1;

                this.itemsCount = newCount;

                if (watchingEnd)
                {
                    int min = itemsCount - range.Diff - 1;
                    int max = itemsCount - 1;
                    if (range.Min != min || range.Max != max)
                        VisibleRange = new IndexRange(min, itemsCount - 1);
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

    public class DynamicLableProvider : TradeChartAxisLabelProvider
    {
        private DateTime _previousValue;
        private bool _isFirstValue = true;
        private const string _annualTemplate = "d MMM yyyy";
        private const string _monthlyTemplate = "d MMM";
        private const string _dailyTemplate = "d MMM HH:mm";
        private const string _subDailyTemplate = "HH:mm";

        public override void OnBeginAxisDraw()
        {
            base.OnBeginAxisDraw();
            _previousValue = DateTime.MinValue;
            _isFirstValue = true;
        }


        public override string FormatLabel(IComparable dataValue)
        {
            var currentValue = (DateTime)dataValue;
            var range = ParentAxis.VisibleRange;
            var template = "";

            if (_isFirstValue)
            {
                template = _annualTemplate;
                _isFirstValue = false;
            }
            else
            {
                if (DifferentYears(currentValue, _previousValue))
                {
                    template = _annualTemplate;
                }
                else if (DifferentMonths(currentValue, _previousValue) || DifferentDays(currentValue, _previousValue))
                {
                    template = IsBeginningOfDay(currentValue) ? _monthlyTemplate : _dailyTemplate;
                }
                else
                {
                    template = _subDailyTemplate;
                }
            }

            _previousValue = (DateTime)dataValue;
            return currentValue.ToString(template);
        }

        private bool DifferentDays(DateTime date1, DateTime date2)
        {
            return date1.DayOfYear != date2.DayOfYear;
        }
        private bool DifferentMonths(DateTime date1, DateTime date2)
        {
            return date1.Month != date2.Month;
        }
        private bool DifferentYears(DateTime date1, DateTime date2)
        {
            return date1.Year - date2.Year != 0;
        }
        private bool IsBeginningOfDay(DateTime dt)
        {
            return dt.TimeOfDay == TimeSpan.FromHours(0);
        }
    }
}

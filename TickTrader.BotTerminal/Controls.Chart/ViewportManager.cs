//using Machinarium.Var;
//using SciChart.Charting.ChartModifiers;
//using SciChart.Charting.ViewportManagers;
//using SciChart.Charting.Visuals;
//using SciChart.Charting.Visuals.Axes;
//using SciChart.Data.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;

//namespace TickTrader.BotTerminal
//{
//    internal class ViewportManager : DefaultViewportManager
//    {
//        private double _selectedZoom = 1;
//        private double _rightLimitExt = 100;
//        private IRange _prevRange;

//        public ViewportManager()
//        {
//        }

//        public XDragModifier XDragModifier { get; internal set; }

//        public double Zoom
//        {
//            get => _selectedZoom;
//            set
//            {
//                if (_selectedZoom != value)
//                {
//                    _selectedZoom = value;
//                    UpdateAxis();
//                }
//            }
//        }

//        public double RightLimitExt
//        {
//            get => _rightLimitExt;
//            set
//            {
//                if (_rightLimitExt != value)
//                {
//                    _rightLimitExt = value;
//                    UpdateAxis();
//                }
//            }
//        }

//        public double MinZoom { get; set; }
//        public double MaxZoom { get; set; }
//        public bool KeepAtMax { get; set; }

//        public event Action ZoomScaleChanged;
//        public event Action<IAxis, IRange, IRange> AxisRangeUpdated;
//        public event Action MovedOutOfMax;

//        public void ScrollTo(long newIndex)
//        {
//            var surface = XDragModifier?.ParentSurface;
//            var axis = surface?.XAxis;

//            MovedOutOfMax?.Invoke();

//            if (axis != null && axis.VisibleRange is IndexRange)
//            {
//                var currentRange = (IndexRange)axis.VisibleRange;
//                var diff = (int)newIndex - currentRange.Min;
//                var newRange = new IndexRange((int)newIndex, currentRange.Max + diff);

//                if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
//                    UpdateTo(axis, axis.DataRange, newRange);
//            }
//            else if (axis != null && axis.VisibleRange is DateRange)
//            {
//                var currentRange = (DateRange)axis.VisibleRange;
//                var diff = newIndex - currentRange.Min.Ticks;
//                var newRange = new DateRange(currentRange.Min.AddTicks(diff), currentRange.Max.AddTicks(diff));

//                if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
//                    UpdateTo(axis, axis.DataRange, newRange);
//            }
//        }

//        public void ScrollToMax()
//        {
//            var surface = XDragModifier?.ParentSurface;
//            var axis = surface?.XAxis;

//            if (axis != null && axis.VisibleRange is IndexRange)
//            {
//                var newRange = CalcMaxVisibleRange(axis);

//                //System.Diagnostics.Debug.WriteLine("ScrollToMax() new=" + newRange.Min + "-" + newRange.Max);

//                if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
//                    UpdateTo(axis, axis.DataRange, newRange);
//            }
//        }

//        private IRange CalcMaxVisibleRange(IAxis axis)
//        {
//            if (axis.VisibleRange is IndexRange)
//            {
//                var currentRange = (IndexRange)axis.VisibleRange;
//                var limits = (IndexRange)axis.VisibleRangeLimit;
//                var diff = limits.Max - currentRange.Max;
//                return new IndexRange(currentRange.Min + diff, limits.Max);
//            }
//            else
//            {
//                var currentRange = (DateRange)axis.VisibleRange;
//                var limits = (DateRange)axis.VisibleRangeLimit;
//                var diff = limits.Max - currentRange.Max;
//                return new DateRange(currentRange.Min + diff, limits.Max);
//            }
//        }

//        protected override IRange OnCalculateNewXRange(IAxis xAxis)
//        {
//            var dataRange = xAxis.DataRange;
//            var visRange = xAxis.VisibleRange;
//            var viewWidth = xAxis.Width;

//            if (viewWidth != 0 && dataRange != null && visRange != null && (visRange is IndexRange || visRange is DateRange))
//            {
//                if (XDragModifier?.IsDragging == true)
//                {
//                    var diff = visRange is IndexRange
//                        ? (visRange as IndexRange).Diff
//                        : (visRange as DateRange).Diff.Ticks / 10_000_000;
//                    // zoom changed
//                    var reqestedZoom = viewWidth / diff;
//                    _selectedZoom = LimitZoom(reqestedZoom);
//                    ZoomScaleChanged?.Invoke();
//                }

//                if ((_prevRange is DateRange && visRange is IndexRange) || (_prevRange is IndexRange && visRange is DateRange))
//                    _prevRange = null;

//                if (_selectedZoom != 0)
//                {
//                    if (_prevRange != null && _prevRange.Max.CompareTo(visRange.Max) > 0)
//                        MovedOutOfMax?.Invoke();

//                    var result = RecalculateRange(viewWidth, dataRange, visRange, out var limit);

//                    //System.Diagnostics.Debug.WriteLine("OnCalculateNewXRange() in=[{0}-{1}] out=[{2}-{3}]",
//                    //    visRange.Min, visRange.Max, result.Min, result.Max);

//                    _prevRange = result;

//                    if (!Equals(xAxis.VisibleRangeLimit, limit))
//                        xAxis.VisibleRangeLimit = limit;

//                    AxisRangeUpdated?.Invoke(xAxis, result, limit);
//                    return result;
//                }
//            }

//            return base.OnCalculateNewXRange(xAxis);
//        }

//        private IRange RecalculateRange(double viewWidth, IRange dataRange, IRange visRange, out IRange rangeLimit)
//        {
//            IRange res;
//            if (visRange is IndexRange)
//            {
//                res = RecalculateIndexRange(viewWidth, (IndexRange)dataRange, (IndexRange)visRange, out var indexLimit);
//                rangeLimit = indexLimit;
//            }
//            else
//            {
//                res = RecalculateDateRange(viewWidth, (DateRange)dataRange, (DateRange)visRange, out var dateLimit);
//                rangeLimit = dateLimit;
//            }

//            return res;
//        }

//        private IndexRange RecalculateIndexRange(double viewWidth, IndexRange dataRange, IndexRange visRange, out IndexRange rangeLimit)
//        {
//            var zoom = _selectedZoom;
//            var reqTotalWidth = dataRange.Diff * zoom;
//            var targetVisDiff = (int)(viewWidth / zoom);

//            var ext = _rightLimitExt;

//            if (reqTotalWidth <= viewWidth - ext )
//            {
//                rangeLimit = new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
//                return new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
//            }
//            else
//            {
//                var targetLimitDiff = (int)((reqTotalWidth + ext) / zoom);
//                rangeLimit = new IndexRange(dataRange.Min, dataRange.Min + targetLimitDiff);

//                var newVisMin = visRange.Max - targetVisDiff;
//                var newVisMax = visRange.Max;

//                if (newVisMin < rangeLimit.Min)
//                    return new IndexRange(rangeLimit.Min, rangeLimit.Min + targetVisDiff);
//                if (KeepAtMax || newVisMax > rangeLimit.Max)
//                    return new IndexRange(rangeLimit.Max - targetVisDiff, rangeLimit.Max);

//                return new IndexRange(newVisMin, newVisMax);
//            }
//        }

//        private DateRange RecalculateDateRange(double viewWidth, DateRange dataRange, DateRange visRange, out DateRange rangeLimit)
//        {
//            var zoom = _selectedZoom;
//            var reqTotalWidth = (dataRange.Diff.Ticks / 10_000_000) * zoom;
//            var targetVisDiff = (long)(viewWidth / zoom) * 10_000_000;

//            var ext = _rightLimitExt;

//            if (reqTotalWidth <= viewWidth - ext)
//            {
//                rangeLimit = new DateRange(dataRange.Min, dataRange.Min.AddTicks(targetVisDiff));
//                return new DateRange(dataRange.Min, dataRange.Min.AddTicks(targetVisDiff));
//            }
//            else
//            {
//                var targetLimitDiff = (long)((reqTotalWidth + ext) / zoom) * 10_000_000;
//                rangeLimit = new DateRange(dataRange.Min, dataRange.Min.AddTicks(targetLimitDiff));

//                var newVisMin = visRange.Max.AddTicks(-targetVisDiff);
//                var newVisMax = visRange.Max;

//                if (newVisMin < rangeLimit.Min)
//                    return new DateRange(rangeLimit.Min, rangeLimit.Min.AddTicks(targetVisDiff));
//                if (KeepAtMax || newVisMax > rangeLimit.Max)
//                    return new DateRange(rangeLimit.Max.AddTicks(-targetVisDiff), rangeLimit.Max);

//                return new DateRange(newVisMin, newVisMax);
//            }
//        }

//        private double LimitZoom(double currentZoom)
//        {
//            if (currentZoom < MinZoom)
//                return MinZoom;
//            else if (currentZoom > MaxZoom)
//                return MaxZoom;
//            return currentZoom;
//        }

//        private void UpdateTo(IAxis axis, IRange dataRange, IRange visRange)
//        {
//            axis.VisibleRange = RecalculateRange(axis.Width, dataRange, visRange, out var limit);
//            if (!Equals(axis.VisibleRangeLimit, limit))
//                axis.VisibleRangeLimit = limit;
//        }

//        private void UpdateAxis()
//        {
//            var surface = XDragModifier?.ParentSurface;
//            var axis = surface?.XAxis;

//            if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
//                UpdateTo(axis, axis.DataRange, axis.VisibleRange);
//        }
//    }
//}

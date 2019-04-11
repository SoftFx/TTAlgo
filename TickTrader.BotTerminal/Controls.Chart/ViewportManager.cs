using Machinarium.Var;
using SciChart.Charting.ChartModifiers;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TickTrader.BotTerminal
{
    internal class ViewportManager : DefaultViewportManager
    {
        private double _selectedZoom = 1;
        private double _rightLimitExt = 100;
        private IndexRange _prevRange;

        public ViewportManager()
        {
        }

        public XDragModifier XDragModifier { get; internal set; }

        public double Zoom
        {
            get => _selectedZoom;
            set
            {
                if (_selectedZoom != value)
                {
                    _selectedZoom = value;
                    UpdateAxis();
                }
            }
        }

        public double RightLimitExt
        {
            get => _rightLimitExt;
            set
            {
                if (_rightLimitExt != value)
                {
                    _rightLimitExt = value;
                    UpdateAxis();
                }
            }
        }

        public double MinZoom { get; set; }
        public double MaxZoom { get; set; }
        public bool KeepAtMax { get; set; }

        public event Action ZoomScaleChanged;
        public event Action<IAxis, IndexRange, IndexRange> AxisRangeUpdated;
        public event Action MovedOutOfMax;

        public void ScrollTo(int newIndex)
        {
            var surface = XDragModifier?.ParentSurface;
            var axis = surface?.XAxis;

            MovedOutOfMax?.Invoke();

            if (axis != null && axis.VisibleRange is IndexRange)
            {
                var currentRange = (IndexRange)axis.VisibleRange;
                var diff = newIndex - currentRange.Min;
                var newRange = new IndexRange(newIndex, currentRange.Max + diff);

                if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
                    UpdateTo(axis, (IndexRange)axis.DataRange, newRange);
            }
        }

        public void ScrollToMax()
        {
            var surface = XDragModifier?.ParentSurface;
            var axis = surface?.XAxis;

            if (axis != null && axis.VisibleRange is IndexRange)
            {
                var newRange = CalcMaxVisibleRange(axis);

                //System.Diagnostics.Debug.WriteLine("ScrollToMax() new=" + newRange.Min + "-" + newRange.Max);

                if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
                    UpdateTo(axis, (IndexRange)axis.DataRange, newRange);
            }
        }

        private IndexRange CalcMaxVisibleRange(IAxis axis)
        {
            var currentRange = (IndexRange)axis.VisibleRange;
            var limits = (IndexRange)axis.VisibleRangeLimit;
            var diff = limits.Max - currentRange.Max;
            return new IndexRange(currentRange.Min + diff, limits.Max);
        }

        protected override IRange OnCalculateNewXRange(IAxis xAxis)
        {
            var dataRange = (IndexRange)xAxis.DataRange;
            var visRange = (IndexRange)xAxis.VisibleRange;
            var viewWidth = xAxis.Width;

            if (viewWidth != 0 && dataRange != null && visRange != null)
            {
                if (XDragModifier?.IsDragging == true)
                {
                    // zoom changed
                    var reqestedZoom = viewWidth / visRange.Diff;
                    _selectedZoom = LimitZoom(reqestedZoom);
                    ZoomScaleChanged?.Invoke();
                }

                if (_selectedZoom != 0)
                {
                    if (_prevRange != null && _prevRange.Max > visRange.Max)
                        MovedOutOfMax?.Invoke();

                    IndexRange limit;
                    var result = RecalculateRange(viewWidth, dataRange, visRange, out limit);

                    //System.Diagnostics.Debug.WriteLine("OnCalculateNewXRange() in=[{0}-{1}] out=[{2}-{3}]",
                    //    visRange.Min, visRange.Max, result.Min, result.Max);

                    _prevRange = result;

                    if (!Equals(xAxis.VisibleRangeLimit, limit))
                        xAxis.VisibleRangeLimit = limit;

                    AxisRangeUpdated?.Invoke(xAxis, result, limit);
                    return result;
                }
            }

            return base.OnCalculateNewXRange(xAxis);
        }

        private IndexRange RecalculateRange(double viewWidth, IndexRange dataRange, IndexRange visRange, out IndexRange rangeLimit)
        {
            var reqTotalWidth = dataRange.Diff * _selectedZoom;
            var targetVisDiff = (int)(viewWidth / _selectedZoom);

            var ext = _rightLimitExt;
            var indexExt = (int)(ext / _selectedZoom);

            if (reqTotalWidth <= viewWidth - ext )
            {
                rangeLimit = new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
                return new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
            }
            else
            {
                var targetLimitDiff = (int)((reqTotalWidth + ext) / _selectedZoom);
                rangeLimit = new IndexRange(dataRange.Min, dataRange.Min + targetLimitDiff);

                var newVisMin = visRange.Max - targetVisDiff;
                var newVisMax = visRange.Max;

                if (newVisMin < rangeLimit.Min)
                    return new IndexRange(rangeLimit.Min, rangeLimit.Min + targetVisDiff);
                if (KeepAtMax || newVisMax > rangeLimit.Max)
                    return new IndexRange(rangeLimit.Max - targetVisDiff, rangeLimit.Max);

                return new IndexRange(newVisMin, newVisMax);
            }
        }

        private double LimitZoom(double currentZoom)
        {
            if (currentZoom < MinZoom)
                return MinZoom;
            else if (currentZoom > MaxZoom)
                return MaxZoom;
            return currentZoom;
        }

        private void UpdateTo(IAxis axis, IndexRange dataRange, IndexRange visRange)
        {
            IndexRange limit;
            axis.VisibleRange = RecalculateRange(axis.Width, dataRange, visRange, out limit);
            if (!Equals(axis.VisibleRangeLimit, limit))
                axis.VisibleRangeLimit = limit;
        }

        private void UpdateAxis()
        {
            var surface = XDragModifier?.ParentSurface;
            var axis = surface?.XAxis;

            if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
                UpdateTo(axis, (IndexRange)axis.DataRange, (IndexRange)axis.VisibleRange);
        }

        private static bool Equals(IndexRange range1, IndexRange range2)
        {
            if (range1 == null)
                return range2 == null;
            else if (range2 == null)
                return false;

            return range1.Max == range2.Max && range1.Min == range2.Min;
        }
    }
}

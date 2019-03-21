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
    class ViewportManager : DefaultViewportManager
    {
        private double _selectedZoom;

        public XDragModifier XDragModifier { get; internal set; }

        public double Zoom
        {
            get => _selectedZoom;
            set
            {
                if (_selectedZoom != value)
                {
                    _selectedZoom = value;
                    UpdateZoom();
                }
            }
        }

        public double MinZoom { get; set; }
        public double MaxZoom { get; set; }

        public event Action ZoomScaleChanged;

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
                    return RecalculateRange(viewWidth, dataRange, visRange);
            }

            return base.OnCalculateNewXRange(xAxis);
        }

        private IndexRange RecalculateRange(double viewWidth, IndexRange dataRange, IndexRange visRange)
        {
            var targetDataWidth = dataRange.Diff * _selectedZoom;
            var targetVisDiff = (int)(viewWidth / _selectedZoom);

            if (targetDataWidth <= viewWidth)
                return new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
            else
            {
                var newVisMin = visRange.Max - targetVisDiff;
                var newVisMax = visRange.Max;

                if (newVisMin < dataRange.Min)
                    return new IndexRange(dataRange.Min, dataRange.Min + targetVisDiff);
                else if (newVisMax > dataRange.Max)
                    return new IndexRange(dataRange.Max - targetVisDiff, dataRange.Max);

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

        private void UpdateZoom()
        {
            var surface = XDragModifier?.ParentSurface;
            var axis = surface?.XAxis;

            if (axis != null && axis.DataRange != null && axis.VisibleRange != null)
                axis.VisibleRange = RecalculateRange(axis.Width, (IndexRange)axis.DataRange, (IndexRange)axis.VisibleRange);
        }
    }
}

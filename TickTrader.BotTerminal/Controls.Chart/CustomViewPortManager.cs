using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class CustomViewPortManager : DefaultViewportManager
    {
        const int maxZoomRatio = 10;
        const int minZoomRatio = 1;

        private double? oldWidth;
        private double? oldZoom;

        protected override IRange OnCalculateNewXRange(IAxis xAxis)
        {
            //System.Diagnostics.Debug.WriteLine("XRange w=" + xAxis.Width + " min=" + xAxis.DataRange.Min + " max=" + xAxis.DataRange.Max + " vmin=" + xAxis.VisibleRange.Min + " vmax=" + xAxis.VisibleRange.Max);

            if (xAxis is CategoryDateTimeAxis)
                return CalculateXIndexRange(xAxis);
            else
                return base.OnCalculateNewXRange(xAxis);
        }

        private IRange CalculateXIndexRange(IAxis xAxis)
        {
            var totalRange = (IndexRange)xAxis.DataRange;
            var visRange = (IndexRange)xAxis.VisibleRange;

            IRange result;

            if (totalRange == null || totalRange.Diff == 0)
                result = xAxis.VisibleRange;
            else
            {
                var zoom = xAxis.Width / (double)visRange.Diff;

                if (oldWidth == null)
                {
                    oldWidth = xAxis.Width;
                    oldZoom = zoom;
                    return xAxis.VisibleRange;
                }
                else
                {
                    if (oldWidth != xAxis.Width)
                    {
                        //System.Diagnostics.Debug.WriteLine("Width changed! w=" + xAxis.Width + " z=" + zoom);
                        result = new IndexRange(visRange.Max - (int)(xAxis.Width / oldZoom), visRange.Max );
                    }
                    else if (oldZoom.Value != zoom)
                    {
                        //System.Diagnostics.Debug.WriteLine("Zoom changed! w=" + xAxis.Width + " z=" + zoom);
                        result = xAxis.VisibleRange;
                        oldZoom = zoom;
                    }
                    else
                    {
                        result = xAxis.VisibleRange;
                    }

                    oldWidth = xAxis.Width;
                }
            }

            return result;
        }

        private IRange CalculateNewRange(double zoom, double visibleMin, double totalDiff)
        {
            return new DoubleRange(visibleMin, visibleMin + totalDiff / zoom);
        }
    }
}

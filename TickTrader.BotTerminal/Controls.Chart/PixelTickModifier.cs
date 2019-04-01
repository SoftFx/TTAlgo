using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class PixelTickModifier : AxisModifierBase
    {
        private List<IAxis> _axes = new List<IAxis>();

        public static readonly DependencyProperty MinDeltaProperty =
            DependencyProperty.Register("MinDelta", typeof(double), typeof(PixelTickModifier),
                new PropertyMetadata(30d, MinDeltaPropertyChanged));

        public double MinDelta
        {
            get { return (double)GetValue(MinDeltaProperty); }
            set { SetValue(MinDeltaProperty, value); }
        }

        private static void MinDeltaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var modifier = (PixelTickModifier)d;
            foreach (var axis in modifier._axes)
                modifier.UpdateAxisTicks(axis);
        }

        public override void OnAttached()
        {
            var customViewPort = ParentSurface.ViewportManager as ViewportManager;
            if (customViewPort != null)
                customViewPort.AxisRangeUpdated += ViewPort_AxisRangeUpdated;

            base.OnAttached();
        }

        public override void OnDetached()
        {
            var customViewPort = ParentSurface.ViewportManager as ViewportManager;
            if (customViewPort != null)
                customViewPort.AxisRangeUpdated -= ViewPort_AxisRangeUpdated;

            base.OnDetached();
        }

        private void ViewPort_AxisRangeUpdated(ViewportManager viewport, IAxis axis)
        {
            //System.Diagnostics.Debug.WriteLine("CustomViewPort_AxisWidthChanged w=" + axis.Width);
            UpdateAxisTicks(axis);
        }

        protected override void AttachTo(IAxis axis)
        {
            if (!_axes.Contains(axis))
                _axes.Add(axis);
            axis.AutoTicks = false;
            UpdateAxisTicks(axis);
        }

        protected override void DeattachFrom(IAxis axis)
        {
            _axes.Remove(axis);
        }

        private void UpdateAxisTicks(IAxis axis)
        {
            var maxTicks = (int)Math.Floor(axis.Width / MinDelta);
            if (maxTicks <= 0)
                maxTicks = 1;
            axis.MaxAutoTicks = maxTicks;



            //var range = axis.VisibleRange as IndexRange;
            //if (range != null)
            //{
            //    var dataPointWidth = axis.Width / range.Diff;

            //    var delta = (int)Math.Floor(MinDelta / dataPointWidth);
            //    if (delta <= 0)
            //        delta = 1;

            //    axis.MinorDelta = delta / 2;
            //    axis.MajorDelta = delta;
        }

        //System.Diagnostics.Debug.WriteLine("MaxTicks=" + maxTicks + " mDelta=" + axis.MajorDelta);
    }
}

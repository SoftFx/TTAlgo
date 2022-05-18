//using SciChart.Charting.ChartModifiers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace TickTrader.BotTerminal
//{
//    class ZoomControl : ChartModifierBase
//    {
//        private bool _isUpdating;
//        private ViewportManager _viewport;

//        public static readonly DependencyProperty ZoomScaleProperty =
//            DependencyProperty.Register("ZoomScale", typeof(double), typeof(ZoomControl), new PropertyMetadata(default(double), OnZoomScaleChanged));

//        public double ZoomScale
//        {
//            get { return (double)GetValue(ZoomScaleProperty); }
//            set { SetValue(ZoomScaleProperty, value); }
//        }

//        public static readonly DependencyProperty MaxZoomScaleProperty =
//            DependencyProperty.Register("MaxZoomScale", typeof(double), typeof(ZoomControl), new PropertyMetadata(default(double), OnMaxZoomScaleChanged));

//        public double MaxZoomScale
//        {
//            get { return (double)GetValue(MaxZoomScaleProperty); }
//            set { SetValue(MaxZoomScaleProperty, value); }
//        }

//        public static readonly DependencyProperty MinZoomScaleProperty =
//            DependencyProperty.Register("MinZoomScale", typeof(double), typeof(ZoomControl), new PropertyMetadata(default(double), OnMinZoomScaleChanged));

//        public double MinZoomScale
//        {
//            get { return (double)GetValue(MinZoomScaleProperty); }
//            set { SetValue(MinZoomScaleProperty, value); }
//        }

//        private static void OnZoomScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var modifier = (ZoomControl)d;
//            if (modifier._isUpdating)
//                return;
//            if (modifier._viewport != null)
//                modifier._viewport.Zoom = (double)e.NewValue;
//        }

//        private static void OnMaxZoomScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var modifier = (ZoomControl)d;
//            if (modifier._viewport != null)
//                modifier._viewport.MaxZoom = (double)e.NewValue;
//        }

//        private static void OnMinZoomScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var modifier = (ZoomControl)d;
//            if (modifier._viewport != null)
//                modifier._viewport.MinZoom = (double)e.NewValue;
//        }

//        public override void OnAttached()
//        {
//            _viewport = ParentSurface.ViewportManager as ViewportManager;

//            if (_viewport != null)
//            {
//                _viewport.MinZoom = MinZoomScale;
//                _viewport.MaxZoom = MaxZoomScale;
//                _viewport.Zoom = ZoomScale;

//                _viewport.ZoomScaleChanged += _viewport_ZoomScaleChanged;
//            }

//            base.OnAttached();
//        }

//        public override void OnDetached()
//        {
//            _viewport = null;
//            base.OnDetached();
//        }

//        private void _viewport_ZoomScaleChanged()
//        {
//            _isUpdating = true;
//            ZoomScale = _viewport.Zoom;
//            _isUpdating = false;
//        }
//    }
//}

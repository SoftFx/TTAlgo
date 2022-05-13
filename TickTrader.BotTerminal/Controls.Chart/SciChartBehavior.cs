//using SciChart.Charting.Visuals;
//using SciChart.Charting.Visuals.Axes;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Media;

//namespace TickTrader.BotTerminal
//{
//    public class SciChartBehavior
//    {
//        public static readonly DependencyProperty XAxisProperty = DependencyProperty.RegisterAttached(
//            "XAxis", typeof(IAxis), typeof(SciChartBehavior), new PropertyMetadata(OnXAxisChanged));

//        public static readonly DependencyProperty XAxisStyleProperty = DependencyProperty.RegisterAttached(
//            "XAxisStyle", typeof(Style), typeof(SciChartBehavior), new PropertyMetadata(OnXAxisStyleChanged));

//        public static readonly DependencyProperty GridLinesColorProperty = DependencyProperty.RegisterAttached(
//            "GridLinesColor", typeof(Brush), typeof(SciChartBehavior), new FrameworkPropertyMetadata(
//                null, FrameworkPropertyMetadataOptions.Inherits));

//        public static void SetXAxis(DependencyObject element, IAxis value)
//        {
//            element.SetValue(XAxisProperty, value);
//        }

//        public static IAxis GetXAxis(DependencyObject element)
//        {
//            return (IAxis)element.GetValue(XAxisProperty);
//        }

//        public static void SetXAxisStyle(DependencyObject element, Style value)
//        {
//            element.SetValue(XAxisStyleProperty, value);
//        }

//        public static Style GetXAxisStyle(DependencyObject element)
//        {
//            return (Style)element.GetValue(XAxisStyleProperty);
//        }

//        public static void SetGridLinesColor(DependencyObject element, Brush value)
//        {
//            element.SetValue(GridLinesColorProperty, value);
//        }

//        public static Brush GetGridLinesColor(DependencyObject element)
//        {
//            return (Brush)element.GetValue(GridLinesColorProperty);
//        }

//        private static void OnXAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var scs = d as SciChartSurface;
//            if (scs == null) return;
//            scs.XAxis = (IAxis)e.NewValue;
//            if (e.NewValue != null)
//            {
//                var style = GetXAxisStyle(d);
//                ((AxisBase)e.NewValue).Style = style;
//            }
//        }

//        private static void OnXAxisStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var scs = d as SciChartSurface;
//            if (scs == null) return;

//            var axis = GetXAxis(d);
//            if (axis != null)
//            {
//                ((AxisBase)axis).Style = (Style)e.NewValue;
//            }   
//        }
//    }
//}

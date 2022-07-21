//using Machinarium.Var;
//using SciChart.Charting.ChartModifiers;
//using SciChart.Charting.Visuals.Axes;
//using SciChart.Data.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace TickTrader.BotTerminal
//{
//    public class AutoScrollModifier : ChartModifierBase
//    {
//        private IAxis _axis;
//        private ViewportManager _viewport;

//        public override void OnAttached()
//        {
//            ParentSurface.XAxes.CollectionChanged += XAxes_CollectionChanged;

//            _viewport = ParentSurface.ViewportManager as ViewportManager;

//            if (_viewport != null)
//            {
//                _viewport.MovedOutOfMax += _viewport_MovedOutOfMax;
//                _viewport.KeepAtMax = IsEnabled;
//            }

//            base.OnAttached();
//        }

//        private void _viewport_MovedOutOfMax()
//        {
//            if (IsEnabled || _viewport.KeepAtMax)
//            {
//                _viewport.KeepAtMax = false;
//                IsEnabled = false;
//            }
//        }

//        protected override void OnIsEnabledChanged()
//        {
//            if (_viewport != null)
//            {
//                if (IsEnabled)
//                {
//                    _viewport.KeepAtMax = true;
//                    _viewport.ScrollToMax();
//                }
//                else
//                    _viewport.KeepAtMax = false;
//            }
//        }

//        public override void OnDetached()
//        {
//            base.OnDetached();
//            ParentSurface.XAxes.CollectionChanged -= XAxes_CollectionChanged;
//            if (_viewport != null)
//            {
//                _viewport.KeepAtMax = false;
//                _viewport.MovedOutOfMax -= _viewport_MovedOutOfMax;
//            }
//        }

//        private void XAxes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
//        {
//            if (_axis != ParentSurface.XAxis)
//                _axis = ParentSurface.XAxis;
//        }
//    }
//}

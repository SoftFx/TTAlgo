using Machinarium.Var;
using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class ScrollBindingModifier : ChartModifierBase
    {
        private IAxis _axis;
        private ViewportManager _viewport;
        private bool _isUpdating;
        private VarContext _context = new VarContext();
        private DoubleProperty _positionProp;
        private DoubleProperty _viewportSizeProp;
        private DoubleProperty _maximumProp;
        private DoubleProperty _minimumProp;

        public DoubleProperty Position => _positionProp;
        public DoubleVar ViewportSize => _viewportSizeProp.Var;
        public DoubleVar Maximum => _maximumProp.Var;
        public DoubleVar Minimum => _minimumProp.Var;

        public ScrollBindingModifier()
        {
            _positionProp = _context.AddDoubleProperty();
            _viewportSizeProp = _context.AddDoubleProperty();
            _maximumProp = _context.AddDoubleProperty();
            _minimumProp = _context.AddDoubleProperty();

            _context.TriggerOnChange(_positionProp, a => UpdateAxis());
        }

        public override void OnAttached()
        {
            base.OnAttached();
            ParentSurface.XAxes.CollectionChanged += XAxes_CollectionChanged;

            _viewport = ParentSurface.ViewportManager as ViewportManager;

            if (_viewport != null)
                _viewport.AxisRangeUpdated += _viewport_AxisRangeUpdated;
        }

        private void _viewport_AxisRangeUpdated(IAxis axis, IRange range, IRange limit)
        {
            if (!_isUpdating && limit != null && range != null)
            {
                var pos = range is IndexRange ? (range as IndexRange).Min : (range as DateRange).Min.Ticks;
                var vSize = range is IndexRange ? (range as IndexRange).Diff : (range as DateRange).Diff.Ticks;
                var limitMax = limit is IndexRange ? (limit as IndexRange).Max : (limit as DateRange).Max.Ticks;
                var limitMin = limit is IndexRange ? (limit as IndexRange).Min : (limit as DateRange).Min.Ticks;
                var scrollMax = limitMax - vSize;

                try
                {
                    _isUpdating = true;

                    if (_minimumProp.Value != limitMin)
                        _minimumProp.Value = limitMin;
                    if (_maximumProp.Value != scrollMax)
                        _maximumProp.Value = scrollMax;

                    if (_positionProp.Value != pos)
                        _positionProp.Value = pos;

                    if (_viewportSizeProp.Value != vSize)
                        _viewportSizeProp.Value = vSize;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        private void UpdateAxis()
        {
            if (_isUpdating)
                return;

            try
            {
                _isUpdating = true;
                _viewport?.ScrollTo((long)_positionProp.Value);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        public override void OnDetached()
        {
            base.OnDetached();
            ParentSurface.XAxes.CollectionChanged -= XAxes_CollectionChanged;
            if (_viewport != null)
                _viewport.AxisRangeUpdated -= _viewport_AxisRangeUpdated;
        }

        private void XAxes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_axis != ParentSurface.XAxis)
                _axis = ParentSurface.XAxis;
        }
    }
}

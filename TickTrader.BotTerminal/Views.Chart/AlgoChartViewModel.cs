using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoChartViewModel : PropertyChangedBase, IDisposable
    {
        private VarContext _var = new VarContext();
        private IntProperty _precisionProp = new IntProperty();
        private Property<IRateInfo> _currentRateProp = new Property<IRateInfo>();
        private Property<double?> _askProp = new Property<double?>();
        private Property<double?> _bidProp = new Property<double?>();
        private Property<Feed.Types.Timeframe> _timeframeProp = new Property<Feed.Types.Timeframe>();
        private IDisposable _axisBind;
        private IDisposable _currentRateBind;

        public AlgoChartViewModel(IVarList<IRenderableSeriesViewModel> dataSeries)
        {
            //var dataSeries = DataSeries;
            var overlaySeries = OutputGroups.Chain().SelectMany(i => i.OverlaySeries);
            var allSeries = VarCollection.CombineChained(dataSeries, overlaySeries);
            var allPanes = OutputGroups.Chain().SelectMany(i => i.Panes);

            DataSeries = dataSeries.AsObservable();
            Series = allSeries.AsObservable();
            Panes = allPanes.AsObservable();

            OutputGroups.Updated += AllOutputs_Updated;

            _var.TriggerOnChange(SymbolInfo, a => UpdatePrecision());

            _var.TriggerOnChange(_currentRateProp, a =>
            {
                _askProp.Value = a.New?.DoubleNullableAsk();
                _bidProp.Value = a.New?.DoubleNullableBid();
            });

            InitZoom();
        }
        
        public IReadOnlyList<IRenderableSeriesViewModel> DataSeries { get; }
        public IReadOnlyList<OutputPaneViewModel> Panes { get; }
        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; }
        public VarList<OutputGroupViewModel> OutputGroups { get; } = new VarList<OutputGroupViewModel>();
        public Property<string> ChartWindowId { get; } = new Property<string>();
        public BoolProperty AutoScroll { get; } = new BoolProperty();
        public Property<AxisBase> TimeAxis { get; } = new Property<AxisBase>();
        public bool ShowScrollbar { get; set; }
        public object Overlay { get; set; }

        public BoolProperty IsCrosshairEnabled { get; } = new BoolProperty();
        public Var<double?> CurrentAsk => _askProp.Var;
        public Var<double?> CurrentBid => _bidProp.Var;
        public Property<SymbolInfo> SymbolInfo { get; } = new Property<SymbolInfo>();
        public int Precision { get; private set; }
        public Property<string> YAxisLabelFormat { get; } = new Property<string>();
        public Var<Feed.Types.Timeframe> Timeframe => _timeframeProp.Var;

        public void BindCurrentRate(Var<IRateInfo> rateSrc)
        {
            _currentRateBind?.Dispose();
            _currentRateBind = _var.TriggerOnChange(rateSrc, a => _currentRateProp.Value = a.New);
        }

        public void SetCurrentRate(IRateInfo rate)
        {
            _currentRateProp.Value = rate;
        }

        public void BindAxis(Var<AxisBase> axis)
        {
            _axisBind?.Dispose();
            _axisBind = _var.TriggerOnChange(axis, a => TimeAxis.Value = a.New);
        }

        public void SetTimeframe(Feed.Types.Timeframe timeframe)
        {
            _timeframeProp.Value = timeframe;
        }

        public void Dispose()
        {
            _var.Dispose();
        }

        #region Zoom control

        private static readonly double[] _zooms = new double[] { 1, 1.5, 3, 5, 8, 14 };

        public DoubleProperty Zoom { get; private set; }
        public double MaxZoom => _zooms.Last();
        public double MinZoom => _zooms.First();
        public bool CanZoomIn { get; private set; }
        public bool CanZoomOut{ get; private set; }

        private void InitZoom()
        {
            Zoom = _var.AddDoubleProperty();
            Zoom.Value = _zooms[3];

            _var.TriggerOnChange(Zoom, a =>
            {
                CanZoomIn = Zoom.Value < MaxZoom;
                NotifyOfPropertyChange(nameof(CanZoomIn));
                CanZoomOut = Zoom.Value > MinZoom; 
                NotifyOfPropertyChange(nameof(CanZoomOut));
            });
        }

        public void ZoomIn()
        {
            var index = GetNearestZoomIndex(Zoom.Value);

            if (index < _zooms.Length - 1)
                Zoom.Value = _zooms[index + 1];
        }

        public void ZoomOut()
        {
            var index = GetNearestZoomIndex(Zoom.Value);

            if (index > 0)
                Zoom.Value = _zooms[index - 1];
        }

        private int GetNearestZoomIndex(double currentZoom)
        {
            if (currentZoom < _zooms[0])
                return 0;

            for (int i = 0; i < _zooms.Length; i++)
            {
                var z1 = _zooms[i];

                if (currentZoom < z1)
                {
                    var z0 = _zooms[i - 1];
                    var rate = (currentZoom - z0) / (z1 - z0);

                    if (rate <= 0.5)
                        return i - 1;
                    else
                        return i;
                }
            }

            return _zooms.Length - 1;
        }

        #endregion

        private void UpdatePrecision()
        {
            Precision = SymbolInfo.Value?.Digits ?? 2;
            foreach (var o in OutputGroups.Values)
            {
                Precision = Math.Max(Precision, o.Precision);
            }
            UpdateLabelFormat();
        }

        private void AllOutputs_Updated(ListUpdateArgs<OutputGroupViewModel> args)
        {
            if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Replace)
            {
                if (args.NewItem != null)
                    args.NewItem.PrecisionUpdated += UpdatePrecision;
            }
            if (args.Action == DLinqAction.Replace || args.Action == DLinqAction.Remove)
            {
                if (args.OldItem != null)
                    args.OldItem.PrecisionUpdated -= UpdatePrecision;
            }
            UpdatePrecision();
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat.Value = $"n{Precision}";
        }
    }
}

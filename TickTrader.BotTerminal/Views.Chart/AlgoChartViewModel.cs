using Machinarium.Qnil;
using Machinarium.Var;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class AlgoChartViewModel : EntityBase
    {
        private IntProperty _precisionProp = new IntProperty();
        private Property<QuoteEntity> _currentRateProp = new Property<QuoteEntity>();
        private Property<double?> _askProp = new Property<double?>();
        private Property<double?> _bidProp = new Property<double?>();
        private IDisposable _axisBind;
        private IDisposable _currentRateBind;

        public AlgoChartViewModel(IVarList<IRenderableSeriesViewModel> dataSeries)
        {
            //var dataSeries = DataSeries;
            var overlaySeries = OutputGroups.Chain().SelectMany(i => i.OverlaySeries);
            var allSeries = VarCollection.CombineChained(dataSeries, overlaySeries);
            var allPanes = OutputGroups.Chain().SelectMany(i => i.Panes);

            Series = allSeries.AsObservable();
            Panes = allPanes.AsObservable();

            OutputGroups.Updated += AllOutputs_Updated;

            TriggerOnChange(SymbolInfo, a => UpdatePrecision());

            TriggerOnChange(_currentRateProp, a =>
            {
                _askProp.Value = a.New?.GetNullableAsk();
                _bidProp.Value = a.New?.GetNullableBid();
            });
        }

        public IReadOnlyList<OutputPaneViewModel> Panes { get; }
        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; }
        public VarList<OutputGroupViewModel> OutputGroups { get; } = new VarList<OutputGroupViewModel>();
        public Property<string> ChartWindowId { get; } = new Property<string>();
        //public VarList<IRenderableSeriesViewModel> DataSeries { get; } = new VarList<IRenderableSeriesViewModel>();

        public Property<AxisBase> TimeAxis { get; } = new Property<AxisBase>();

        public object Overlay { get; set; }

        public BoolProperty IsCrosshairEnabled { get; } = new BoolProperty();
        public Var<double?> CurrentAsk => _askProp.Var;
        public Var<double?> CurrentBid => _bidProp.Var;
        public Property<SymbolEntity> SymbolInfo { get; } = new Property<SymbolEntity>();
        public int Precision { get; private set; }
        public Property<string> YAxisLabelFormat { get; } = new Property<string>();

        public void BindCurrentRate(Var<QuoteEntity> rateSrc)
        {
            _currentRateBind?.Dispose();
            _currentRateBind = TriggerOnChange(rateSrc, a => _currentRateProp.Value = a.New);
        }

        public void BindAxis(Var<AxisBase> axis)
        {
            _axisBind?.Dispose();
            _axisBind = TriggerOnChange(axis, a => TimeAxis.Value = a.New);
        }

        private void UpdatePrecision()
        {
            Precision = SymbolInfo.Value?.Precision ?? 2;
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

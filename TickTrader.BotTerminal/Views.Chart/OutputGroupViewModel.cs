using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
//using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class OutputGroupViewModel
    {
        private IPluginDataChartModel _chart;
        private List<OutputSeriesModel> _outputModels;
        private ISymbolInfo _symbol;
        private VarList<OutputSeriesModel> _overlayOutputs;
        //private VarList<IRenderableSeriesViewModel> _overlaySeries;
        private VarList<OutputPaneViewModel> _panes;
        private BoolVar _isCrosshairEnabled;

        public IPluginModel Model { get; }

        //public string DisplayName => Model.InstanceId;

        public string ChartWindowId { get; }

        public IVarList<OutputSeriesModel> OverlayOutputs => _overlayOutputs;

        public IVarList<OutputPaneViewModel> Panes => _panes;

        //public IVarList<IRenderableSeriesViewModel> OverlaySeries => _overlaySeries;

        public int Precision { get; private set; }

        public event System.Action PrecisionUpdated;

        public OutputGroupViewModel(IPluginModel plugin, string windowId, IPluginDataChartModel chart, ISymbolInfo symbol,
            BoolVar isCrosshairEnabled)
        {
            Model = plugin;
            ChartWindowId = windowId;
            _chart = chart;
            _symbol = symbol;
            _isCrosshairEnabled = isCrosshairEnabled;

            _overlayOutputs = new VarList<OutputSeriesModel>();
            _panes = new VarList<OutputPaneViewModel>();
            //_overlaySeries = new VarList<IRenderableSeriesViewModel>();

            Init();

            Model.OutputsChanged += ModelOnOutputsChanged;
        }

        public void Dispose()
        {
            Model.OutputsChanged -= ModelOnOutputsChanged;

            Deinit();
        }

        private void Init()
        {
            //_outputModels = CreateOutputModels(Model).ToList();

            _outputModels.Where(o => o.Descriptor.Target == Metadata.Types.OutputTarget.Overlay).ForEach(_overlayOutputs.Add);
            //_overlayOutputs.Values.ForEach(o => _overlaySeries.Add(SeriesViewModel.FromOutputSeries(o)));

            foreach (Metadata.Types.OutputTarget target in Enum.GetValues(typeof(Metadata.Types.OutputTarget)))
            {
                if (target != Metadata.Types.OutputTarget.Overlay)
                {
                    if (_outputModels.Any(o => o.Descriptor.Target == target))
                    {
                        _panes.Add(new OutputPaneViewModel(Model, _outputModels, ChartWindowId, _chart, _symbol, target, _isCrosshairEnabled));
                    }
                }
            }

            UpdatePrecision();
        }

        private void Deinit()
        {
            _overlayOutputs.Clear();
            //_overlaySeries.Clear();
            foreach (var pane in _panes.Values)
            {
                pane.Dispose();
            }
            _panes.Clear();

            DisposeOutputModels();
        }

        private void DisposeOutputModels()
        {
            if (_outputModels != null)
            {
                _outputModels.ForEach(m => m.Dispose());
                _outputModels = null;
            }
        }

        //private IEnumerable<OutputSeriesModel> CreateOutputModels(IPluginModel plugin)
        //{
        //    //foreach (var outputCollector in plugin.Outputs.Values)
        //    //{
        //    //    var config = outputCollector.OutputConfig;

        //    //    if (config is ColoredLineOutputConfig)
        //    //        yield return new DoubleSeriesModel(_chart, outputCollector);
        //    //    else if (config is MarkerSeriesOutputConfig)
        //    //        yield return new MarkerSeriesModel(_chart, outputCollector);
        //    //}
        //}

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in _overlayOutputs.Values)
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Digits : output.Descriptor.Precision);
            }
            PrecisionUpdated?.Invoke();
        }

        private void ModelOnOutputsChanged()
        {
            Execute.OnUIThread(() =>
            {
                Deinit();
                Init();
            });
        }
    }
}

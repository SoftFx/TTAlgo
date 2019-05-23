using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class OutputGroupViewModel
    {
        private IPluginDataChartModel _chart;
        private List<OutputSeriesModel> _outputModels;
        private SymbolEntity _symbol;
        private VarList<OutputSeriesModel> _overlayOutputs;
        private VarList<IRenderableSeriesViewModel> _overlaySeries;
        private VarList<OutputPaneViewModel> _panes;
        private BoolVar _isCrosshairEnabled;

        public IPluginModel Model { get; }

        //public string DisplayName => Model.InstanceId;

        public string ChartWindowId { get; }

        public IVarList<OutputSeriesModel> OverlayOutputs => _overlayOutputs;

        public IVarList<OutputPaneViewModel> Panes => _panes;

        public IVarList<IRenderableSeriesViewModel> OverlaySeries => _overlaySeries;

        public int Precision { get; private set; }

        public event System.Action PrecisionUpdated;

        public OutputGroupViewModel(IPluginModel plugin, string windowId, IPluginDataChartModel chart, SymbolEntity symbol,
            BoolVar isCrosshairEnabled)
        {
            Model = plugin;
            ChartWindowId = windowId;
            _chart = chart;
            _symbol = symbol;
            _isCrosshairEnabled = isCrosshairEnabled;

            _overlayOutputs = new VarList<OutputSeriesModel>();
            _panes = new VarList<OutputPaneViewModel>();
            _overlaySeries = new VarList<IRenderableSeriesViewModel>();

            Init();

            Model.OutputsChanged += ModelOnOutputsChanged;
        }

        public void Dispose()
        {
            Model.OutputsChanged -= ModelOnOutputsChanged;

            DisposeOutputModels();
        }

        private void Init()
        {
            DisposeOutputModels();

            _outputModels = CreateOutputModels(Model).ToList();

            _outputModels.Where(o => o.Descriptor.Target == OutputTargets.Overlay).Foreach(_overlayOutputs.Add);
            _overlayOutputs.Values.Foreach(o => _overlaySeries.Add(SeriesViewModel.FromOutputSeries(o)));

            foreach (OutputTargets target in Enum.GetValues(typeof(OutputTargets)))
            {
                if (target != OutputTargets.Overlay)
                {
                    if (_outputModels.Any(o => o.Descriptor.Target == target))
                    {
                        _panes.Add(new OutputPaneViewModel(Model, _outputModels, ChartWindowId, _chart, _symbol, target, _isCrosshairEnabled));
                    }
                }
            }

            UpdatePrecision();
        }

        private void DisposeOutputModels()
        {
            if (_outputModels != null)
            {
                _outputModels.ForEach(m => m.Dispose());
                _outputModels = null;
            }
        }

        private IEnumerable<OutputSeriesModel> CreateOutputModels(IPluginModel plugin)
        {
            foreach (var outputCollector in plugin.Outputs.Values)
            {
                var outputSetup = outputCollector.OutputConfig;

                if (outputSetup is ColoredLineOutputSetupModel)
                    yield return new DoubleSeriesModel(plugin, _chart, outputCollector, (ColoredLineOutputSetupModel)outputSetup);
                else if (outputSetup is MarkerSeriesOutputSetupModel)
                    yield return new MarkerSeriesModel(plugin, _chart, outputCollector, (MarkerSeriesOutputSetupModel)outputSetup);
            }
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in _overlayOutputs.Values)
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Precision : output.Descriptor.Precision);
            }
            PrecisionUpdated?.Invoke();
        }

        private void ModelOnOutputsChanged()
        {
            Execute.OnUIThread(() =>
            {
                _overlayOutputs.Clear();
                _overlaySeries.Clear();
                _panes.Clear();
                Init();
            });
        }
    }
}

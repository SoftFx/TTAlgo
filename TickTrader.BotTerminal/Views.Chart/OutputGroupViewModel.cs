using Caliburn.Micro;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class OutputGroupViewModel
    {
        private ChartModelBase _chart;
        private SymbolModel _symbol;
        private VarList<OutputSeriesModel> _overlayOutputs;
        private VarList<IRenderableSeriesViewModel> _overlaySeries;
        private VarList<OutputPaneViewModel> _panes;


        public PluginModel Model { get; }

        public string DisplayName => Model.InstanceId;

        public string ChartWindowId { get; }

        public IVarList<OutputSeriesModel> OverlayOutputs => _overlayOutputs;

        public IVarList<OutputPaneViewModel> Panes => _panes;

        public IVarList<IRenderableSeriesViewModel> OverlaySeries => _overlaySeries;

        public int Precision { get; private set; }


        public event System.Action PrecisionUpdated;


        public OutputGroupViewModel(PluginModel plugin, string windowId, ChartModelBase chart, SymbolModel symbol)
        {
            Model = plugin;
            ChartWindowId = windowId;
            _chart = chart;
            _symbol = symbol;

            _overlayOutputs = new VarList<OutputSeriesModel>();
            _panes = new VarList<OutputPaneViewModel>();
            _overlaySeries = new VarList<IRenderableSeriesViewModel>();

            Init();

            Model.OutputsChanged += ModelOnOutputsChanged;
        }


        public void Dispose()
        {
            Model.OutputsChanged -= ModelOnOutputsChanged;
        }


        private void Init()
        {
            Model.Outputs.Values.Where(o => o.Descriptor.Target == OutputTargets.Overlay).Foreach(_overlayOutputs.Add);
            _overlayOutputs.Values.Foreach(o => _overlaySeries.Add(SeriesViewModel.FromOutputSeries(o)));

            foreach (OutputTargets target in Enum.GetValues(typeof(OutputTargets)))
            {
                if (target != OutputTargets.Overlay)
                {
                    if (Model.Outputs.Values.Any(o => o.Descriptor.Target == target))
                    {
                        _panes.Add(new OutputPaneViewModel(Model, ChartWindowId, _chart, _symbol, target));
                    }
                }
            }

            UpdatePrecision();
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in _overlayOutputs.Values)
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Descriptor.Precision : output.Descriptor.Precision);
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

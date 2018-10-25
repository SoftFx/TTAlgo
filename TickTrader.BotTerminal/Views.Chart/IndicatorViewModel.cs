using Caliburn.Micro;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase _chart;
        private SymbolModel _symbol;

        public IndicatorViewModel(ChartModelBase chart, IndicatorModel indicator, string windowId, SymbolModel symbol)
        {
            _chart = chart;
            Model = indicator;
            ChartWindowId = windowId;
            _symbol = symbol;
            OverlayOutputs = new VarList<OutputSeriesModel>();
            Panes = new VarList<IndicatorPaneViewModel>();
            OverlaySeries = OverlayOutputs.Select(SeriesViewModel.FromOutputSeries);

            Init();

            Model.OutputsChanged += ModelOnOutputsChanged;
        }

        public IndicatorModel Model { get; private set; }
        public string DisplayName { get { return Model.InstanceId; } }
        public VarList<OutputSeriesModel> OverlayOutputs { get; private set; }
        public IVarList<IRenderableSeriesViewModel> OverlaySeries { get; private set; }
        public VarList<IndicatorPaneViewModel> Panes { get; private set; }
        public string ChartWindowId { get; private set; }
        public int Precision { get; private set; }

        public void Close()
        {
            _chart.RemoveIndicator(Model);
        }


        private void Init()
        {
            Model.Outputs.Values.Where(o => o.Descriptor.Target == OutputTargets.Overlay).Foreach(OverlayOutputs.Add);

            foreach (OutputTargets target in Enum.GetValues(typeof(OutputTargets)))
            {
                if (target != OutputTargets.Overlay)
                {
                    if (Model.Outputs.Values.Any(o => o.Descriptor.Target == target))
                    {
                        Panes.Add(new IndicatorPaneViewModel(this, _chart, target, _symbol));
                    }
                }
            }

            UpdatePrecision();
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in OverlayOutputs.Values)
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Descriptor.Precision : output.Descriptor.Precision);
            }
        }

        private void ModelOnOutputsChanged()
        {
            Execute.OnUIThread(() =>
            {
                OverlayOutputs.Clear();
                Panes.Clear();
                Init();
            });
        }
    }
}

using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase _chart;

        public IndicatorViewModel(ChartModelBase chart, IndicatorModel indicator, string windowId, SymbolModel symbol)
        {
            _chart = chart;
            ChartWindowId = windowId;
            Model = indicator;
            Series = new DynamicList<IRenderableSeriesViewModel>();
            Panes = new DynamicList<IndicatorPaneViewModel>();
            Precision = 0;

            foreach (OutputSetupModel output in indicator.Setup.Outputs.Where(o => o.Descriptor.Target == OutputTargets.Overlay))
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? symbol.Descriptor.Precision : output.Descriptor.Precision);
                var seriesViewModel = SeriesViewModel.CreateIndicatorSeries(indicator, output);
                if (seriesViewModel != null)
                    Series.Values.Add(seriesViewModel);
            }

            foreach (OutputTargets target in Enum.GetValues(typeof(OutputTargets)))
            {
                if (target != OutputTargets.Overlay)
                {
                    CreatePane(target, symbol);
                }
            }
        }

        public IndicatorModel Model { get; private set; }
        public string DisplayName { get { return Model.InstanceId; } }
        public DynamicList<IRenderableSeriesViewModel> Series { get; private set; }
        public DynamicList<IndicatorPaneViewModel> Panes { get; private set; }
        public string ChartWindowId { get; private set; }
        public int Precision { get; private set; }

        public void Close()
        {
            _chart.RemoveIndicator(Model);
        }


        private void CreatePane(OutputTargets target, SymbolModel symbol)
        {
            if (Model.Setup.Outputs.Any(o => o.Descriptor.Target == target))
            {
                Panes.Values.Add(new IndicatorPaneViewModel(this, _chart, target, symbol));
            }
        }
    }
}

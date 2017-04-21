using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase chart;

        public IndicatorViewModel(ChartModelBase chart, IndicatorModel indicator)
        {
            this.chart = chart;
            Model = indicator;
            Series = new DynamicList<IRenderableSeriesViewModel>();

            foreach (OutputSetup output in indicator.Setup.Outputs.Where(o => o.IsOverlay))
            {
                var seriesViewModel = SeriesViewModel.CreateIndicatorSeries(indicator, output);
                if (seriesViewModel != null)
                    Series.Values.Add(seriesViewModel);
            }
        }

        public IndicatorModel Model { get; private set; }
        public string DisplayName { get { return Model.Name; } }
        public DynamicList<IRenderableSeriesViewModel> Series { get; private set; }

        public void Close()
        {
            chart.RemoveIndicator(Model);
        }
    }
}

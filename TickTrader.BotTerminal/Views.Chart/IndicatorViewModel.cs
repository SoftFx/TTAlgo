using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase chart;

        public IndicatorViewModel(ChartModelBase chart, IndicatorModel2 indicator)
        {
            this.chart = chart;
            Model = indicator;
            Series = new DynamicList<IRenderableSeriesViewModel>();

            foreach (OutputSetup output in indicator.Setup.Outputs)
            {
                var seriesViewModel = SeriesViewModel.CreateIndicatorSeries(indicator, output);
                if (seriesViewModel != null)
                    Series.Values.Add(seriesViewModel);
            }
        }

        public IndicatorModel2 Model { get; private set; }
        public string DisplayName { get { return Model.Name; } }
        public DynamicList<IRenderableSeriesViewModel> Series { get; private set; }

        public void Close()
        {
            chart.RemoveIndicator(Model);
        }
    }
}

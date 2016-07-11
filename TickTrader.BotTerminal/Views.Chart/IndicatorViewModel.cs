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
        public IndicatorViewModel(IndicatorModel indicator)
        {
            Model = indicator;
            Series = new DynamicList<IRenderableSeriesViewModel>();

            foreach (OutputSetup output in indicator.Setup.Outputs)
            {
                var seriesViewModel = SeriesViewModel.Create(indicator, output);
                if (seriesViewModel != null)
                    Series.Values.Add(seriesViewModel);
            }
        }

        public IndicatorModel Model { get; private set; }
        public string DisplayName { get { return "[" + Model.Id + "] " + Model.DisplayName; } }
        public DynamicList<IRenderableSeriesViewModel> Series { get; private set; }

        public void Close()
        {
            Model.Close();
        }
    }
}

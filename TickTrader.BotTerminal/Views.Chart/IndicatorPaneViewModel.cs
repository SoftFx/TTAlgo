using Caliburn.Micro;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    class IndicatorPaneViewModel : PropertyChangedBase
    {
        private IndicatorModel indicator;
        private ChartModelBase chart;
        private ObservableCollection<IRenderableSeries> series = new ObservableCollection<IRenderableSeries>();

        public IndicatorPaneViewModel(IndicatorModel indicator, ChartModelBase chartModel)
        {
            this.chart = chartModel;
            this.indicator = indicator;

            foreach (var output in indicator.SeriesCollection)
                Series.Add(output);

            UpdateAxis();
            chart.NavigatorChanged += UpdateAxis;
        }

        public string Header { get; private set; }

        public void Close()
        {
            chart.NavigatorChanged -= UpdateAxis;
        }

        private void UpdateAxis()
        {
            TimeAxis = chart.Navigator.CreateAxis();
            TimeAxis.Visibility = System.Windows.Visibility.Collapsed;
            NotifyOfPropertyChange("TimeAxis");
        }

        public AxisBase TimeAxis { get; private set; }
        public long IndicatorId { get { return indicator.Id; } }
        public ChartModelBase Chart { get { return chart; } }
        public ObservableCollection<IRenderableSeries> Series { get { return series; } }
    }
}

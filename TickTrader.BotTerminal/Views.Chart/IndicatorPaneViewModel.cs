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
            {
                FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
                chartSeries.DataSeries = output;
                Series.Add(chartSeries);
            }

            UpdateAxis();
            chart.NavigatorChanged += UpdateAxis;

            //XyDataSeries<int> dataSeries = new XyDataSeries<int>();
            //dataSeries.Append(Enumerable.Range(0, 100), Enumerable.Range(0, 100));

            //FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
            //chartSeries.DataSeries = dataSeries;
            //Series.Add(chartSeries);
            
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

using Caliburn.Micro;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartData;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
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
        private IndicatorViewModel indicator;
        private ChartModelBase chart;

        public IndicatorPaneViewModel(IndicatorViewModel indicator, ChartModelBase chartModel, string windowId)
        {
            this.chart = chartModel;
            this.indicator = indicator;
            this.ChartWindowId = windowId;

            Series = indicator.Series.AsObservable();

            UpdateAxis();
            chart.NavigatorChanged += UpdateAxis;

            //Annotations = new AnnotationCollection();

            //var markers = indicator.Annotations.SelectMany(mSeries => mSeries);
            //markers.ConnectTo(Annotations);
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

        public string ChartWindowId { get; private set; }
        public AxisBase TimeAxis { get; private set; }
        //public long IndicatorId { get { return indicator.Model.Id; } }
        public ChartModelBase Chart { get { return chart; } }
        public IObservableListSource<IRenderableSeriesViewModel> Series { get; private set; }
        //public AnnotationCollection Annotations { get; private set; }
    }
}

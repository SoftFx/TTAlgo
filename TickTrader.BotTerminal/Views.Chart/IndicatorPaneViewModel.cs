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
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    class IndicatorPaneViewModel : PropertyChangedBase
    {
        private IndicatorViewModel _indicator;
        private ChartModelBase _chart;

        public IndicatorPaneViewModel(IndicatorViewModel indicator, ChartModelBase chartModel, OutputTargets target, SymbolModel symbol)
        {
            _chart = chartModel;
            _indicator = indicator;
            ChartWindowId = indicator.ChartWindowId;
            Precision = 0;

            var series = new VarList<IRenderableSeriesViewModel>();
            foreach (OutputSetupModel output in indicator.Model.Setup.Outputs.Where(o => o.Metadata.Descriptor.Target == target))
            {
                Precision = Math.Max(Precision, output.Metadata.Descriptor.Precision == -1 ? symbol.Descriptor.Precision : output.Metadata.Descriptor.Precision);
                var seriesViewModel = SeriesViewModel.CreateIndicatorSeries(indicator.Model, output);
                if (seriesViewModel != null)
                    series.Values.Add(seriesViewModel);
            }

            Series = series.AsObservable();

            UpdateAxis();
            UpdateLabelFormat();

            //Annotations = new AnnotationCollection();

            //var markers = indicator.Annotations.SelectMany(mSeries => mSeries);
            //markers.ConnectTo(Annotations);
        }

        public string Header { get; private set; }

        public void Close()
        {
        }

        private void UpdateAxis()
        {
            TimeAxis = _chart.Navigator.CreateAxis();
            TimeAxis.Visibility = System.Windows.Visibility.Collapsed;
            NotifyOfPropertyChange("TimeAxis");
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat = $"n{Precision}";
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }

        public string ChartWindowId { get; private set; }
        public AxisBase TimeAxis { get; private set; }
        //public long IndicatorId { get { return indicator.Model.Id; } }
        public ChartModelBase Chart { get { return _chart; } }
        public IObservableList<IRenderableSeriesViewModel> Series { get; private set; }
        //public AnnotationCollection Annotations { get; private set; }
        public int Precision { get; private set; }
        public string YAxisLabelFormat { get; private set; }
    }
}

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
        private SymbolModel _symbol;

        public IndicatorPaneViewModel(IndicatorViewModel indicator, ChartModelBase chartModel, OutputTargets target, SymbolModel symbol)
        {
            _chart = chartModel;
            _indicator = indicator;
            ChartWindowId = indicator.ChartWindowId;
            _symbol = symbol;
            Outputs = new VarList<OutputSeriesModel>();
            Series = Outputs.Select(SeriesViewModel.FromOutputSeries).AsObservable();

            indicator.Model.Outputs.Values.Where(o => o.Descriptor.Target == target).Foreach(Outputs.Add);

            UpdateAxis();
            UpdatePrecision();

            //Annotations = new AnnotationCollection();

            //var markers = indicator.Annotations.SelectMany(mSeries => mSeries);
            //markers.ConnectTo(Annotations);
        }

        public string ChartWindowId { get; private set; }
        public AxisBase TimeAxis { get; private set; }
        //public long IndicatorId { get { return indicator.Model.Id; } }
        public ChartModelBase Chart { get { return _chart; } }
        public VarList<OutputSeriesModel> Outputs { get; private set; }
        public IObservableList<IRenderableSeriesViewModel> Series { get; private set; }
        //public AnnotationCollection Annotations { get; private set; }
        public int Precision { get; private set; }
        public string YAxisLabelFormat { get; private set; }

        public void Close()
        {
        }


        private void UpdateAxis()
        {
            TimeAxis = _chart.Navigator.CreateAxis();
            TimeAxis.Visibility = System.Windows.Visibility.Collapsed;
            NotifyOfPropertyChange("TimeAxis");
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in Outputs.Values)
            {
                Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Descriptor.Precision : output.Descriptor.Precision);
            }
            UpdateLabelFormat();
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat = $"n{Precision}";
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }
    }
}

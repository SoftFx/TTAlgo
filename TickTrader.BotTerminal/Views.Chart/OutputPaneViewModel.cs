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
    internal class OutputPaneViewModel : PropertyChangedBase
    {
        private SymbolModel _symbol;
        private VarList<OutputSeriesModel> _outputs;

        public PluginModel Model { get; }

        public string DisplayName => Model.InstanceId;

        public string ChartWindowId { get; }

        public ChartModelBase Chart { get; }

        public AxisBase TimeAxis { get; private set; }

        public IVarList<OutputSeriesModel> Outputs => _outputs;

        public IObservableList<IRenderableSeriesViewModel> Series { get; private set; }

        public int Precision { get; private set; }

        public string YAxisLabelFormat { get; private set; }


        public OutputPaneViewModel(PluginModel plugin, string windowId, ChartModelBase chart, SymbolModel symbol, OutputTargets target)
        {
            Model = plugin;
            ChartWindowId = windowId;
            Chart = chart;
            _symbol = symbol;

            _outputs = new VarList<OutputSeriesModel>();
            Series = _outputs.Select(SeriesViewModel.FromOutputSeries).AsObservable();

            Model.Outputs.Values.Where(o => o.Descriptor.Target == target).Foreach(_outputs.Add);

            UpdateAxis();
            UpdatePrecision();
        }


        private void UpdateAxis()
        {
            TimeAxis = Chart.Navigator.CreateAxis();
            Chart.CreateXAxisBinging(TimeAxis);
            TimeAxis.Visibility = System.Windows.Visibility.Collapsed;
            NotifyOfPropertyChange(nameof(TimeAxis));
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            foreach (var output in _outputs.Values)
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

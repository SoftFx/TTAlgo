using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
//using SciChart.Charting.Model.ChartSeries;
//using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class OutputPaneViewModel : PropertyChangedBase
    {
        private ISymbolInfo _symbol;
        private VarList<OutputModel> _outputs;

        public IPluginModel Model { get; }

        public string DisplayName => Model.InstanceId;

        public string ChartWindowId { get; }

        public IPluginDataChartModel Chart { get; }

        //public AxisBase TimeAxis { get; private set; }

        public IVarList<OutputModel> Outputs => _outputs;

        //public IObservableList<IRenderableSeriesViewModel> Series { get; private set; }

        public int Precision { get; private set; }

        public string YAxisLabelFormat { get; private set; }

        public BoolVar IsCrosshairEnabled { get; }

        public OutputPaneViewModel(IPluginModel plugin, IEnumerable<OutputModel> ouputModels, string windowId, IPluginDataChartModel chart,
            ISymbolInfo symbol, Metadata.Types.OutputTarget target, BoolVar isCrosshairEnabled)
        {
            Model = plugin;
            ChartWindowId = windowId;
            Chart = chart;
            _symbol = symbol;
            IsCrosshairEnabled = isCrosshairEnabled;

            _outputs = new VarList<OutputModel>();
            //Series = _outputs.Select(SeriesViewModel.FromOutputSeries).Chain().AsObservable();

            //ouputModels.Where(o => o.Descriptor.Target == target).ForEach(_outputs.Add);

            UpdateAxis();
            UpdatePrecision();
        }

        public void Dispose()
        {
            //Series.Dispose();
        }

        private void UpdateAxis()
        {
            //TimeAxis = Chart.CreateXAxis();
            //TimeAxis.Visibility = System.Windows.Visibility.Collapsed;
            //NotifyOfPropertyChange(nameof(TimeAxis));
        }

        private void UpdatePrecision()
        {
            Precision = 0;
            //foreach (var output in _outputs.Values)
            //{
            //    Precision = Math.Max(Precision, output.Descriptor.Precision == -1 ? _symbol.Digits : output.Descriptor.Precision);
            //}
            UpdateLabelFormat();
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat = $"n{Precision}";
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }
    }
}

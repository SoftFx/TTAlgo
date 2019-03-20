using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel : ObservableObject, IPluginDataChartModel
    {
        private OhlcDataSeries<DateTime, double> _chartData = new OhlcDataSeries<DateTime, double>();
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        private ChartNavigator _navigator = new UniformChartNavigator();

        public BacktesterChartPageViewModel()
        {
            var mainSeries = new OhlcRenderableSeriesViewModel();
            mainSeries.StyleKey = "BarChart_OhlcStyle";
            mainSeries.DataSeries = _chartData;
            _mainSeriesCollection.Add(mainSeries);

            ChartControlModel = new AlgoChartViewModel(_mainSeriesCollection);
            ChartControlModel.TimeAxis.Value = _navigator.CreateAxis();
            ChartControlModel.ChartWindowId.Value = Guid.NewGuid().ToString();
        }

        public AlgoChartViewModel ChartControlModel { get; }

        public void OnStart(SymbolEntity mainSymbol, PluginSetupModel setup, Backtester backtester)
        {
            Clear();

            backtester.OutputDataMode = TestDataSeriesFlags.Stream;
            backtester.ChartDataMode = TestDataSeriesFlags.Stream;

            backtester.OnChartUpdate += Backtester_OnChartUpdate;

            var adapter = new BacktesterAdapter(setup, backtester);
            var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this, mainSymbol);
            ChartControlModel.OutputGroups.Add(outputGroup);
        }

        private void Backtester_OnChartUpdate(BarEntity bar, string symbol, SeriesUpdateActions action)
        {
            if (action == SeriesUpdateActions.Append)
                _chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
        }

        public void Clear()
        {
            _chartData.Clear();
            ChartControlModel.OutputGroups.Clear();
            //_mainSeriesCollection.Clear();
            //_panes.Clear();
        }

        public void SetFeedSeries(OhlcDataSeries<DateTime, double> chartData)
        {   
        }

        #region IPluginDataChartModel

        ITimeVectorRef IPluginDataChartModel.TimeSyncRef => null;
        bool IExecStateObservable.IsStarted => true;

        event Action IExecStateObservable.StartEvent { add { } remove { } }
        event AsyncEventHandler IExecStateObservable.StopEvent { add { } remove { } }


        AxisBase IPluginDataChartModel.CreateXAxis()
        {
            return _navigator.CreateAxis();
        }

        #endregion
    }

    internal class BacktesterChartPaneModel
    {
        public BacktesterChartPaneModel(BacktesterChartPageViewModel parent, params IRenderableSeriesViewModel[] series)
        {
            Parent = parent;
            Series = series.ToList();
        }

        public BacktesterChartPageViewModel Parent { get; }
        public List<IRenderableSeriesViewModel> Series { get; }
    }
}

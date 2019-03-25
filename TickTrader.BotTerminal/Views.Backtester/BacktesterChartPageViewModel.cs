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
        private ChartBarVector _barVector;
        private OhlcRenderableSeriesViewModel _mainSeries;
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        private ChartNavigator _navigator = new UniformChartNavigator();

        public BacktesterChartPageViewModel()
        {
            _mainSeries = new OhlcRenderableSeriesViewModel();
            _mainSeries.StyleKey = "BarChart_OhlcStyle";
            _mainSeriesCollection.Add(_mainSeries);

            ChartControlModel = new AlgoChartViewModel(_mainSeriesCollection);
            ChartControlModel.TimeAxis.Value = _navigator.CreateAxis();
            ChartControlModel.ChartWindowId.Value = Guid.NewGuid().ToString();
        }

        public AlgoChartViewModel ChartControlModel { get; }

        public void OnStart(bool visualizing, SymbolEntity mainSymbol, PluginSetupModel setup, Backtester backtester)
        {
            Clear();

            _barVector = new ChartBarVector(backtester.MainTimeframe);
            _mainSeries.DataSeries = _barVector.SciChartdata;

            if (visualizing)
            {
                backtester.SymbolDataConfig.Add(mainSymbol.Name, TestDataSeriesFlags.Stream | TestDataSeriesFlags.Realtime);
                backtester.OutputDataMode = TestDataSeriesFlags.Stream | TestDataSeriesFlags.Realtime;

                backtester.Executor.SymbolRateUpdated += Executor_SymbolRateUpdated;
            }
            else
            {
                backtester.SymbolDataConfig.Add(mainSymbol.Name, TestDataSeriesFlags.Stream);
                backtester.OutputDataMode = TestDataSeriesFlags.Stream;
                
                backtester.OnChartUpdate += Backtester_OnChartUpdate;
            }
            
            var adapter = new BacktesterAdapter(setup, backtester);
            var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this, mainSymbol);
            ChartControlModel.OutputGroups.Add(outputGroup);
        }

        public void OnStop(Backtester backtester)
        {
            backtester.OnChartUpdate -= Backtester_OnChartUpdate;
            backtester.Executor.SymbolRateUpdated -= Executor_SymbolRateUpdated;
        }

        private void Backtester_OnChartUpdate(BarEntity bar, string symbol, SeriesUpdateActions action)
        {
            if (action == SeriesUpdateActions.Append)
                _barVector.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
        }

        private void Executor_SymbolRateUpdated(Algo.Api.RateUpdate update)
        {
            ChartControlModel.SetCurrentRate(update);

            if (update is QuoteEntity)
            {
                var q = (QuoteEntity)update;
                _barVector.AppendQuote(q.CreatingTime, q.Bid, 1);
            }
            else if (update is BarRateUpdate)
            {
                var bar = ((BarRateUpdate)update).BidBar;
                if (bar != null)
                    _barVector.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
            }
        }

        public void Clear()
        {
            _barVector = null;
            _mainSeries.DataSeries = null;
            ChartControlModel.OutputGroups.Clear();
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

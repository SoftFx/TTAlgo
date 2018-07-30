using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel : ObservableObject
    {
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        //private VarList<IRenderableSeriesViewModel> _seriesCollection = new VarList<IRenderableSeriesViewModel>();
        private VarList<BacktesterChartPaneModel> _panes = new VarList<BacktesterChartPaneModel>();
        private IRange _visibleRange;

        public BacktesterChartPageViewModel()
        {
            Series = _mainSeriesCollection.AsObservable();
            YAxisLabelFormat = $"n2";
            Panes = _panes.AsObservable();

            _panes.Add(new BacktesterChartPaneModel(this));
        }

        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; private set; }
        public string YAxisLabelFormat { get; private set; }
        public IReadOnlyList<BacktesterChartPaneModel> Panes { get; private set; }

        public IRange VisibleRange
        {
            get => _visibleRange;
            set
            {
                if (_visibleRange != value)
                {
                    _visibleRange = value;
                    NotifyOfPropertyChange(nameof(VisibleRange));
                }
            }
        }

        public void Clear()
        {
            _mainSeriesCollection.Clear();
            _panes.Clear();
        }

        public void AddMainSeries(OhlcDataSeries<DateTime, double> chartData)
        {
            //var viewModel = new OhlcRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_OhlcStyle" };

            //if (_mainSeriesCollection.Count > 0)
            //    _mainSeriesCollection.Clear();

            //_mainSeriesCollection.Add(viewModel);
        }

        public void AddStatSeries(TestingStatistics stats)
        {
            var mainSeries = new OhlcRenderableSeriesViewModel();
            mainSeries.StyleKey = "BarChart_OhlcStyle";
            mainSeries.DataSeries = ToOhlc(stats.MainSymbolHistory);
            _mainSeriesCollection.Add(mainSeries);

            var equitySeries = new OhlcRenderableSeriesViewModel();
            equitySeries.DataSeries = ToOhlc(stats.EquityHistory);
            equitySeries.StyleKey = "EquityChart_OhlcStyle";

            //var marginSeries = new OhlcRenderableSeriesViewModel();
            //marginSeries.DataSeries = ToOhlc(stats.MarginHistory);

            _panes.Add(new BacktesterChartPaneModel(this, equitySeries));
        }

        private OhlcDataSeries<DateTime, double> ToOhlc(List<BarEntity> vector)
        {
            var chartData = new OhlcDataSeries<DateTime, double>();

            foreach (var bar in vector)
                chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);

            return chartData;
        }
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

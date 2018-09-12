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

        public void SetFeedSeries(OhlcDataSeries<DateTime, double> chartData)
        {
            var mainSeries = new OhlcRenderableSeriesViewModel();
            mainSeries.StyleKey = "BarChart_OhlcStyle";
            mainSeries.DataSeries = chartData;
            _mainSeriesCollection.Add(mainSeries);
        }

        public void SetEquitySeries(OhlcDataSeries<DateTime, double> chartData)
        {
            var equitySeries = new OhlcRenderableSeriesViewModel();
            equitySeries.DataSeries = chartData;
            equitySeries.StyleKey = "EquityChart_OhlcStyle";
            _panes.Add(new BacktesterChartPaneModel(this, equitySeries));
        }

        public void SetMarginSeries(OhlcDataSeries<DateTime, double> chartData)
        {
            
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

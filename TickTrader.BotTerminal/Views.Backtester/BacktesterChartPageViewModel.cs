using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel
    {
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        private VarList<IRenderableSeriesViewModel> _seriesCollection = new VarList<IRenderableSeriesViewModel>();

        public BacktesterChartPageViewModel()
        {
            Series = _mainSeriesCollection.AsObservable();
            YAxisLabelFormat = $"n2";
        }

        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; private set; }
        public string YAxisLabelFormat { get; private set; }

        public void Clear()
        {
            _seriesCollection.Clear();
        }

        public void AddMainSeries(OhlcDataSeries<DateTime, double> chartData)
        {
            var viewModel = new OhlcRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_OhlcStyle" };
            if (_mainSeriesCollection.Count > 0)
                _mainSeriesCollection.Clear();

            _mainSeriesCollection.Add(viewModel);
        }
    }
}

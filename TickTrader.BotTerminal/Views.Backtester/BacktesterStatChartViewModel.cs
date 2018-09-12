using Machinarium.Var;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BacktesterStatChartViewModel : ObservableObject
    {
        private List<IRenderableSeriesViewModel> _seriesList = new List<IRenderableSeriesViewModel>();
        private double _maxColumnValue;

        public BacktesterStatChartViewModel(string title, ReportDiagramTypes type)
        {
            Title = title;
            Style = type.ToString();
        }

        public BacktesterStatChartViewModel AddStackedColumns(IReadOnlyList<decimal> data, ReportSeriesStyles style)
        {
            var dataModel = new XyDataSeries<int, double>(data.Count);


            for (int i = 0; i < data.Count; i++)
            {
                var yVal = (double)data[i];

                if (yVal == 0)
                    yVal = -1;

                dataModel.Append(i + 1, yVal);

                if (yVal > _maxColumnValue)
                    _maxColumnValue = yVal;
            }

            var series = new StackedColumnRenderableSeriesViewModel();
            series.StyleKey = style + "Style";
            series.DataSeries = dataModel;
            series.StackedGroupId = style.ToString();

            _seriesList.Add(series);

            if (_maxColumnValue > 0)
                YRange = new DoubleRange(0, _maxColumnValue);
            else
                YRange = new DoubleRange(0, 1);
            NotifyOfPropertyChange(nameof(YRange));

            return this;
        }

        public DoubleRange YRange { get; set; }

        public string Title { get; }
        public string Style { get; }

        public IEnumerable<IRenderableSeriesViewModel> SeriesList => _seriesList;
    }

    public enum ReportDiagramTypes { CategoryHistogram }
    public enum ReportSeriesStyles { ProfitColumns, LossColumns }
}

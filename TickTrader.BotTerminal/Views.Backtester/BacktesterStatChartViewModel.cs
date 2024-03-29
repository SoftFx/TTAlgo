﻿using Caliburn.Micro;
using Machinarium.Var;
//using SciChart.Charting.Model.ChartSeries;
//using SciChart.Charting.Model.DataSeries;
//using SciChart.Charting.Visuals.Axes;
//using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterStatChartViewModel : PropertyChangedBase
    {
        //private List<IRenderableSeriesViewModel> _seriesList = new List<IRenderableSeriesViewModel>();
        private double _maxColumnValue;

        public BacktesterStatChartViewModel(string title, ReportDiagramTypes type)
        {
            Title = title;
            Style = type.ToString();
        }

        //public BacktesterStatChartViewModel AddBarSeries(OhlcDataSeries<DateTime, double> data, ReportSeriesStyles style)
        //{
        //    var series = new OhlcRenderableSeriesViewModel();
        //    series.StyleKey = style + "Style";
        //    series.DataSeries = data;
        //    _seriesList.Add(series);

        //    return this;
        //}

        public BacktesterStatChartViewModel AddStackedColumns(IReadOnlyList<double> data, ReportSeriesStyles style, bool xAxisDayNames)
        {
            //var dataModel = new XyDataSeries<int, double>(data.Count);

            //for (int i = 0; i < data.Count; i++)
            //{
            //    var yVal = data[i];

            //    if (yVal == 0)
            //        yVal = -1;

            //    dataModel.Append(i, yVal);

            //    if (yVal > _maxColumnValue)
            //        _maxColumnValue = yVal;
            //}

            //var series = new StackedColumnRenderableSeriesViewModel();
            //series.StyleKey = style + "Style";
            //series.DataSeries = dataModel;
            //series.StackedGroupId = style.ToString();

            //_seriesList.Add(series);

            //if (_maxColumnValue > 0)
            //    YRange = new DoubleRange(0, _maxColumnValue);
            //else
            //    YRange = new DoubleRange(0, 1);
            //NotifyOfPropertyChange(nameof(YRange));

            //XAxisDayNames = xAxisDayNames;
            //NotifyOfPropertyChange(nameof(XAxisDayNames));

            return this;
        }

        //public DoubleRange YRange { get; set; }
        public bool XAxisDayNames { get; private set; }
        public string Title { get; }
        public string Style { get; }

        //public IEnumerable<IRenderableSeriesViewModel> SeriesList => _seriesList;
    }

    public enum ReportDiagramTypes { CategoryHistogram, CategoryDatetime }
    public enum ReportSeriesStyles { ProfitColumns, LossColumns, Equity, Margin }
}

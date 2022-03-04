using Machinarium.Var;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterReportViewModel : Page
    {
        private readonly VarContext _var = new VarContext();
        private Property<Dictionary<string, string>> _statProperties;
        private IntProperty _depositDigits;
        private OhlcDataSeries<DateTime, double> _equityData;
        private OhlcDataSeries<DateTime, double> _marginData;

        public BacktesterReportViewModel()
        {
            UpdateHeader(null);

            _statProperties = _var.AddProperty<Dictionary<string, string>>();
            _depositDigits = _var.AddIntProperty(2);
        }

        public Var<Dictionary<string, string>> StatProperties => _statProperties.Var;
        public ObservableCollection<BacktesterStatChartViewModel> SmallCharts { get; } = new ObservableCollection<BacktesterStatChartViewModel>();
        public ObservableCollection<BacktesterStatChartViewModel> LargeCharts { get; } = new ObservableCollection<BacktesterStatChartViewModel>();
        public Var<int> DepositDigits => _depositDigits.Var;

        public void Clear()
        {
            UpdateHeader(null);
            _statProperties.Value = null;
            SmallCharts.Clear();
            LargeCharts.Clear();
            _equityData = null;
            _marginData = null;
        }

        public void ShowReport(TestingStatistics newStats, PluginDescriptor descriptor, long? id)
        {
            IsVisible = true;
            UpdateHeader(id);

            var isTradeBot = descriptor.IsTradeBot;

            _statProperties.Value = FormatStats(isTradeBot, newStats);

            if (isTradeBot)
            {
                SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by hours", ReportDiagramTypes.CategoryHistogram)
                    .AddStackedColumns(newStats.ProfitByHours, ReportSeriesStyles.ProfitColumns, false)
                    .AddStackedColumns(newStats.LossByHours, ReportSeriesStyles.LossColumns, false));

                SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by weekdays", ReportDiagramTypes.CategoryHistogram)
                    .AddStackedColumns(newStats.ProfitByWeekDays, ReportSeriesStyles.ProfitColumns, true)
                    .AddStackedColumns(newStats.LossByWeekDays, ReportSeriesStyles.LossColumns, true));
            }
        }

        private static Dictionary<string, string> FormatStats(bool isTradeBot, TestingStatistics stats)
        {
            var props = new Dictionary<string, string>();
            var balanceNumbersFormat = FormatExtentions.CreateTradeFormatInfo(stats.AccBalanceDigits);

            props.Add("Bars", stats.BarsCount.ToString());
            props.Add("Ticks", stats.TicksCount.ToString());

            if (isTradeBot)
            {
                props.Add("Initial deposit", stats.InitialBalance.FormatPlain(balanceNumbersFormat));
                props.Add("Final equity", stats.FinalBalance.FormatPlain(balanceNumbersFormat));
                props.Add("Total profit", (stats.FinalBalance - stats.InitialBalance).FormatPlain(balanceNumbersFormat));
                props.Add("Gross profit", stats.GrossProfit.FormatPlain(balanceNumbersFormat));
                props.Add("Gross loss", stats.GrossLoss.FormatPlain(balanceNumbersFormat));
                props.Add("Commission", stats.TotalComission.FormatPlain(balanceNumbersFormat));
                props.Add("Swap", stats.TotalSwap.FormatPlain(balanceNumbersFormat));
            }

            var elapsed = TimeSpan.FromMilliseconds(stats.ElapsedMs);
            props.Add("Testing time", Format(elapsed));

            var tickPerSecond = "N/A";
            if (elapsed.TotalSeconds > 0)
                tickPerSecond = (stats.TicksCount / elapsed.TotalSeconds).ToString("N0");

            props.Add("Testing speed (tps)", tickPerSecond);

            if (isTradeBot)
            {
                props.Add("Orders opened", stats.OrdersOpened.ToString());
                props.Add("Orders rejected", stats.OrdersRejected.ToString());
                props.Add("Order modifications", stats.OrderModifications.ToString());
                props.Add("Order modifications rejected", stats.OrderModificationRejected.ToString());
            }

            return props;
        }

        public void AddEquityChart(OhlcDataSeries<DateTime, double> bars)
        {
            _equityData = bars;
            var chart = new BacktesterStatChartViewModel("Equity", ReportDiagramTypes.CategoryDatetime);
            chart.AddBarSeries(bars, ReportSeriesStyles.Equity);
            LargeCharts.Add(chart);
        }

        public void AddMarginChart(OhlcDataSeries<DateTime, double> bars)
        {
            _marginData = bars;
            var chart = new BacktesterStatChartViewModel("Margin", ReportDiagramTypes.CategoryDatetime);
            chart.AddBarSeries(bars, ReportSeriesStyles.Margin);
            LargeCharts.Add(chart);
        }

        private static string Format(TimeSpan timeSpan)
        {
            var builder = new StringBuilder();

            int depth = 0;

            int hours = (int)timeSpan.TotalHours;

            if (hours > 0)
            {
                builder.Append(hours).Append("h");
                depth++;
            }

            if (depth < 2 && (timeSpan.Minutes > 0 || depth > 0))
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(timeSpan.Minutes).Append("m");
                depth++;
            }

            if (depth < 2 && (timeSpan.Seconds > 0 || depth > 0))
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(timeSpan.Seconds).Append("s");
                depth++;
            }

            if (depth < 2)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(timeSpan.Milliseconds).Append("ms");
                depth++;
            }

            return builder.ToString();
        }

        private void UpdateHeader(long? id)
        {
            if (id == null)
                DisplayName = "Report";
            else
                DisplayName = "Report: " + id;
        }
    }
}

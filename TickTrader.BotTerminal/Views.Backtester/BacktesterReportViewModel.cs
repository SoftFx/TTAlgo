using Machinarium.Var;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class BacktesterReportViewModel : EntityBase
    {
        private TestingStatistics _stats;
        private Property<Dictionary<string, string>> _statProperties;
        private IntProperty _depositDigits;

        public BacktesterReportViewModel()
        {
            _statProperties = AddProperty<Dictionary<string, string>>();
            _depositDigits = AddIntProperty(2);
            RebuildReport(new TestingStatistics());
        }

        public Var<Dictionary<string, string>> StatProperties => _statProperties.Var;
        public ObservableCollection<BacktesterStatChartViewModel> SmallCharts { get; } = new ObservableCollection<BacktesterStatChartViewModel>();
        public ObservableCollection<BacktesterStatChartViewModel> LargeCharts { get; } = new ObservableCollection<BacktesterStatChartViewModel>();
        public Var<int> DepositDigits => _depositDigits.Var;

        public TestingStatistics Stats
        {
            get => _stats;
            set
            {
                if (_stats != value)
                {
                    _stats = value;
                    RebuildReport(value ?? new TestingStatistics());
                }
            }
        }

        public void Clear()
        {
            Stats = null;
            SmallCharts.Clear();
            LargeCharts.Clear();
        }

        public void SaveAsText(Stream file)
        {
            using (var writer = new StreamWriter(file))
            {
                foreach (var prop in StatProperties.Value)
                {
                    writer.Write(prop.Key);
                    writer.Write(": ");
                    writer.WriteLine(prop.Value);
                }
            }
        }

        private void RebuildReport(TestingStatistics newStats)
        {
            var newPropertis = new Dictionary<string, string>();
            var balanceNumbersFormat = FormatExtentions.CreateTradeFormatInfo(newStats.AccBalanceDigits);

            newPropertis.Add("Bars", newStats.BarsCount.ToString());
            newPropertis.Add("Ticks", newStats.TicksCount.ToString());
            newPropertis.Add("Initial deposit", newStats.InitialBalance.FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Final equity", newStats.FinalBalance.FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Total profit", (newStats.FinalBalance - newStats.InitialBalance).FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Gross profit", newStats.GrossProfit.FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Gross loss", newStats.GrossLoss.FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Commission", newStats.TotalComission.FormatPlain(balanceNumbersFormat));
            newPropertis.Add("Swap", newStats.TotalSwap.FormatPlain(balanceNumbersFormat));

            newPropertis.Add("Testing time", Format(newStats.Elapsed));

            var tickPerSecond = "N/A";
            if (newStats.Elapsed.TotalSeconds > 0)
                tickPerSecond = (newStats.TicksCount / newStats.Elapsed.TotalSeconds).ToString("N0");

            newPropertis.Add("Testing speed (tps)", tickPerSecond);

            newPropertis.Add("Orders opened", newStats.OrdersOpened.ToString());
            newPropertis.Add("Orders rejected", newStats.OrdersRejected.ToString());
            newPropertis.Add("Order modifications", newStats.OrderModifications.ToString());
            newPropertis.Add("Order modifications rejected", newStats.OrderModificationRejected.ToString());

            _statProperties.Value = newPropertis;

            SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by hours", ReportDiagramTypes.CategoryHistogram)
                .AddStackedColumns(newStats.ProfitByHours, ReportSeriesStyles.ProfitColumns)
                .AddStackedColumns(newStats.LossByHours, ReportSeriesStyles.LossColumns));

            SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by weekdays", ReportDiagramTypes.CategoryHistogram)
                .AddStackedColumns(newStats.ProfitByWeekDays, ReportSeriesStyles.ProfitColumns)
                .AddStackedColumns(newStats.LossByWeekDays, ReportSeriesStyles.LossColumns));
        }

        public void AddEquityChart(OhlcDataSeries<DateTime, double> bars)
        {
            var chart = new BacktesterStatChartViewModel("Equity", ReportDiagramTypes.CategoryDatetime);
            chart.AddBarSeries(bars, ReportSeriesStyles.Equity);
            LargeCharts.Add(chart);
        }

        public void AddMarginChart(OhlcDataSeries<DateTime, double> bars)
        {
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
    }
}

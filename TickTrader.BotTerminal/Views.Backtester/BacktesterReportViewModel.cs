using Caliburn.Micro;
using Machinarium.Var;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

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

        public Task SaveMarginCsv(Stream entryStream, IActionObserver observer)
        {
            return SaveCsv(entryStream, observer, "margin", _marginData);
        }

        public Task SaveEquityCsv(Stream entryStream, IActionObserver observer)
        {
            return SaveCsv(entryStream, observer, "equity", _equityData);
        }

        private Task SaveCsv(Stream entryStream, IActionObserver observer, string dataName, OhlcDataSeries<DateTime, double> data)
        {
            observer.SetMessage("Saving " + dataName + "...");

            return Task.Factory.StartNew(() =>
            {
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write("DateTime,Open,High,Low,Close");

                    for (int i = 0; i < data.Count; i++)
                    {
                        writer.WriteLine();
                        writer.Write("{0},{1},{2},{3},{4}", InvariantFormat.CsvFormat(data.XValues[i]), data.OpenValues[i], data.HighValues[i], data.LowValues[i], data.CloseValues[i]);
                    }
                }
            });
        }

        public void ShowReport(TestingStatistics newStats, PluginDescriptor descriptor, long? id)
        {
            IsVisible = true;
            UpdateHeader(id);

            var newPropertis = new Dictionary<string, string>();
            var balanceNumbersFormat = FormatExtentions.CreateTradeFormatInfo(newStats.AccBalanceDigits);

            var pluginType = descriptor.Type;

            newPropertis.Add("Bars", newStats.BarsCount.ToString());
            newPropertis.Add("Ticks", newStats.TicksCount.ToString());

            if (pluginType == AlgoTypes.Robot)
            {
                newPropertis.Add("Initial deposit", newStats.InitialBalance.FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Final equity", newStats.FinalBalance.FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Total profit", (newStats.FinalBalance - newStats.InitialBalance).FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Gross profit", newStats.GrossProfit.FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Gross loss", newStats.GrossLoss.FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Commission", newStats.TotalComission.FormatPlain(balanceNumbersFormat));
                newPropertis.Add("Swap", newStats.TotalSwap.FormatPlain(balanceNumbersFormat));
            }

            newPropertis.Add("Testing time", Format(newStats.Elapsed));

            var tickPerSecond = "N/A";
            if (newStats.Elapsed.TotalSeconds > 0)
                tickPerSecond = (newStats.TicksCount / newStats.Elapsed.TotalSeconds).ToString("N0");

            newPropertis.Add("Testing speed (tps)", tickPerSecond);

            if (pluginType == AlgoTypes.Robot)
            {
                newPropertis.Add("Orders opened", newStats.OrdersOpened.ToString());
                newPropertis.Add("Orders rejected", newStats.OrdersRejected.ToString());
                newPropertis.Add("Order modifications", newStats.OrderModifications.ToString());
                newPropertis.Add("Order modifications rejected", newStats.OrderModificationRejected.ToString());
            }

            _statProperties.Value = newPropertis;

            if (pluginType == AlgoTypes.Robot)
            {
                SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by hours", ReportDiagramTypes.CategoryHistogram)
                    .AddStackedColumns(newStats.ProfitByHours, ReportSeriesStyles.ProfitColumns, false)
                    .AddStackedColumns(newStats.LossByHours, ReportSeriesStyles.LossColumns, false));

                SmallCharts.Add(new BacktesterStatChartViewModel("Profits and losses by weekdays", ReportDiagramTypes.CategoryHistogram)
                    .AddStackedColumns(newStats.ProfitByWeekDays, ReportSeriesStyles.ProfitColumns, true)
                    .AddStackedColumns(newStats.LossByWeekDays, ReportSeriesStyles.LossColumns, true));
            }
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

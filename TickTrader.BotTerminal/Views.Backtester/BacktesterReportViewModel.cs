using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class BacktesterReportViewModel : EntityBase
    {
        private TestingStatistics _stats;
        private Property<Dictionary<string, string>> _statProperties;
        private Property<List<BacktesterStatChartViewModel>> _charts;

        public BacktesterReportViewModel()
        {
            _statProperties = AddProperty<Dictionary<string, string>>();
            _charts = AddProperty<List<BacktesterStatChartViewModel>>();
            RebuildReport(new TestingStatistics());
        }

        public Var<Dictionary<string, string>> StatProperties => _statProperties.Var;
        public Var<List<BacktesterStatChartViewModel>> Charts => _charts.Var;

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
        }

        private void RebuildReport(TestingStatistics newStats)
        {
            var newPropertis = new Dictionary<string, string>();

            newPropertis.Add("Bars", newStats.BarsCount.ToString());
            newPropertis.Add("Ticks", newStats.TicksCount.ToString());
            newPropertis.Add("Initial deposit", newStats.InitialBalance.ToString("N2"));
            newPropertis.Add("Total Net profit", (newStats.FinalBalance - newStats.InitialBalance).ToString("N2"));
            newPropertis.Add("Gross profit", newStats.GrossProfit.ToString("N2"));
            newPropertis.Add("Gross loss", newStats.GrossLoss.ToString("N2"));

            newPropertis.Add("Testing time", newStats.Elapsed.ToString("c"));

            var tickPerSecond = "N/A";
            if (newStats.Elapsed.Seconds > 0)
                tickPerSecond = (newStats.TicksCount / newStats.Elapsed.Seconds).ToString("N0");

            newPropertis.Add("Testing speed (tps)", tickPerSecond);

            newPropertis.Add("Orders opened", newStats.OrdersOpened.ToString());
            newPropertis.Add("Orders rejected", newStats.OrdersRejected.ToString());
            newPropertis.Add("Order modifications", newStats.OrderModifications.ToString());
            newPropertis.Add("Order modifications rejected", newStats.OrderModificationRejected.ToString());

            var newCharts = new List<BacktesterStatChartViewModel>();

            newCharts.Add(new BacktesterStatChartViewModel("Profits and losses by hours", ReportDiagramTypes.CategoryHistogram)
                .AddStackedColumns(newStats.ProfitByHours, ReportSeriesStyles.ProfitColumns)
                .AddStackedColumns(newStats.LossByHours, ReportSeriesStyles.LossColumns));

            newCharts.Add(new BacktesterStatChartViewModel("Profits and losses by weekdays", ReportDiagramTypes.CategoryHistogram)
                .AddStackedColumns(newStats.ProfitByWeekDays, ReportSeriesStyles.ProfitColumns)
                .AddStackedColumns(newStats.LossByWeekDays, ReportSeriesStyles.LossColumns));

            _charts.Value = newCharts;
            _statProperties.Value = newPropertis;
        }
    }
}

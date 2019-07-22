using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class OptimizationResultsPageViewModel : Page
    {
        private readonly Dictionary<string, DataColumn> _idToColumnMap = new Dictionary<string, DataColumn>();
        private readonly Dictionary<long, OptCaseReport> _reports = new Dictionary<long, OptCaseReport>();
        private DataColumn _idColumn;
        private DataColumn _metricColumn;

        public OptimizationResultsPageViewModel()
        {
            DisplayName = "Optimization Results";
            IsVisible = false;
        }

        public event Action<OptCaseReport> ShowDetailsRequested;

        public void Start(IEnumerable<ParameterDescriptor> optParams, Optimizer optimizer)
        {
            IsVisible = true;
            Clear();

            _idColumn = new DataColumn("No", typeof(long));
            Data.Columns.Add(_idColumn);

            foreach (var par in optParams)
            {
                var dataColumn = new DataColumn(par.DisplayName);
                _idToColumnMap.Add(par.Id, dataColumn);
                Data.Columns.Add(dataColumn);
            }

            _metricColumn = new DataColumn("Metric", typeof(double));
            Data.Columns.Add(_metricColumn);
        }

        public void Stop(Optimizer optimizer)
        {
        }

        public void Update(OptCaseReport report)
        {
            _reports[report.Config.Id] = report;

            var row = Data.NewRow();

            foreach (var pair in report.Config)
            {
                if (_idToColumnMap.TryGetValue(pair.Key, out var colNo))
                    row[colNo] = pair.Value;
            }

            row[_idColumn] = report.Config.Id;
            row[_metricColumn] = report.Stats?.FinalBalance ?? -1;

            Data.Rows.Add(row);
        }

        public void Hide()
        {
            IsVisible = false;
            Clear();
        }

        public void ShowReport(DataRowView rowView)
        {
            if (rowView != null)
            {
                var row = rowView.Row;
                var id = (long)row[_idColumn];

                if (_reports.TryGetValue(id, out var report))
                    ShowDetailsRequested?.Invoke(report);
            }
        }

        private void Clear()
        {
            _reports.Clear();
            Data.Clear();
            Data.Columns.Clear();
            _idToColumnMap.Clear();
        }

        public DataTable Data { get; } = new DataTable();
    }
}

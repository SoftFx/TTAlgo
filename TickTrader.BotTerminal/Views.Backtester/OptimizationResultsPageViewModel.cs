using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class OptimizationResultsPageViewModel : Page
    {
        private readonly Dictionary<string, DataColumn> _idToColumnMap = new Dictionary<string, DataColumn>();
        private readonly Dictionary<long, OptCaseReport> _reports = new Dictionary<long, OptCaseReport>();
        private List<ParameterDescriptor> _optParams;
        private DataColumn _idColumn;
        private DataColumn _metricColumn;

        public OptimizationResultsPageViewModel()
        {
            DisplayName = "Optimization Results";
            IsVisible = false;

            DataView = new DataView(Data);
        }

        public DataTable Data { get; } = new DataTable();
        public DataView DataView { get; }

        public event Action<OptCaseReport> ShowDetailsRequested;

        public void Init(IEnumerable<ParameterDescriptor> optParams)
        {
            _optParams = optParams.ToList();

            Clear();

            _idColumn = new DataColumn("No", typeof(long));
            Data.Columns.Add(_idColumn);

            foreach (var par in _optParams)
            {
                var dataColumn = new DataColumn(par.DisplayName);
                _idToColumnMap.Add(par.Id, dataColumn);
                Data.Columns.Add(dataColumn);
            }

            _metricColumn = new DataColumn("Metric", typeof(MetricView));
            Data.Columns.Add(_metricColumn);

            DataView.Sort = _metricColumn.ColumnName + " DESC";
        }

        public void Update(OptCaseReport report)
        {
            _reports[report.Config.Id] = report;

            var row = Data.NewRow();

            foreach (var pair in report.Config.Parameters)
            {
                if (_idToColumnMap.TryGetValue(pair.Key, out var colNo))
                    row[colNo] = pair.Value;
            }

            row[_idColumn] = report.Config.Id;
            row[_metricColumn] = new MetricView(report.MetricVal, GetErrorMessage(report.ExecError));

            Data.Rows.Add(row);
        }

        private string GetErrorMessage(Exception ex)
        {
            if (ex == null)
                return null;
            else if (ex is StopOutException)
                return "Stop Out";
            else if (ex is NotEnoughDataException)
                return "Not Enough Data";

            return "Error";
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
            DataView.Sort = "";
            _reports.Clear();
            Data.Clear();
            Data.Columns.Clear();
            _idToColumnMap.Clear();
        }
    }

    public class MetricView : IComparable
    {
        private bool _hasError;
        private string _displayVal;
        private double _metric;

        public MetricView(double metric, string error = null)
        {
            _metric = metric;

            if (error != null)
            {
                _hasError = true;
                _displayVal = error;
            }
            else
                _displayVal = metric.ToString("g");
        }

        public int CompareTo(object obj)
        {
            var other = (MetricView)obj;
            if (_hasError)
            {
                if (other._hasError)
                    return _metric.CompareTo(other._metric);
                else
                    return -1;
            }
            else
            {
                if (other._hasError)
                    return 1;
                else
                    return _metric.CompareTo(other._metric);
            }
        }

        public override string ToString() => _displayVal;
    }
}

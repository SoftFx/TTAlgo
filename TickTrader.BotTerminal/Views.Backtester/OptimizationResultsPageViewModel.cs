using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Common.Model;
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

            //DataView = Data.AsDataView();
        }

        public DataTable Data { get; } = new DataTable();
        public DataView DataView { get; }

        public event Action<OptCaseReport> ShowDetailsRequested;

        public void Start(IEnumerable<ParameterDescriptor> optParams, Optimizer optimizer)
        {
            _optParams = optParams.ToList();

            IsVisible = true;
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

        public Task SaveReportAsync(PluginDescriptor pDescriptor, IActionObserver observer)
        {
            return Task.Run(() => SaveReport(pDescriptor, observer));
        }

        private void SaveReport(PluginDescriptor pDescriptor, IActionObserver observer)
        {
            var fileName = "Optimization of " + pDescriptor.DisplayName + " " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + ".zip";
            var filePath = System.IO.Path.Combine(EnvService.Instance.BacktestResultsFolder, fileName);

            observer.StartIndeterminateProgress();
            observer.SetMessage("Saving report...");

            using (var stream = System.IO.File.Open(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    var resultsEntry = archive.CreateEntry("resuls.csv", CompressionLevel.Optimal);
                    using (var entryStream = resultsEntry.Open())
                        SaveReports(entryStream);

                    observer.StartProgress(0, _reports.Count);

                    var repNo = 0;

                    foreach (var rep in _reports.Values)
                    {
                        var repFolder = rep.Config.Id + "/";

                        var equityEntry = archive.CreateEntry(repFolder + "equity.csv", CompressionLevel.Optimal);
                        using (var entryStream = equityEntry.Open())
                            BacktesterReportViewModel.SaveCsv(entryStream, rep.Equity);

                        var marginEntry = archive.CreateEntry(repFolder + "margin.csv", CompressionLevel.Optimal);
                        using (var entryStream = marginEntry.Open())
                            BacktesterReportViewModel.SaveCsv(entryStream, rep.Margin);

                        var summaryEntry = archive.CreateEntry(repFolder + "report.txt", CompressionLevel.Optimal);
                        using (var entryStream = summaryEntry.Open())
                            BacktesterReportViewModel.SaveAsText(entryStream, pDescriptor.IsTradeBot, rep.Stats);

                        observer.SetProgress(repNo++);
                    }
                }
            }
        }

        private void SaveReports(Stream stream)
        {
            var reps = _reports.Values.ToList();
            reps.OrderBy(r => r.Stats.FinalBalance);

            var serializer = new CsvSerializer<OptCaseReport>();
            serializer.AddColumn("No", r => r.Config.Id);

            foreach (var p in _optParams)
                serializer.AddColumn(p.DisplayName, r => r.Config.Parameters[p.Id].ToString());

            serializer.AddColumn("Metric", r => r.Stats.FinalBalance);

            serializer.Serialize(reps, stream);
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

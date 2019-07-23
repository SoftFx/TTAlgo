using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

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
        }

        public DataTable Data { get; } = new DataTable();

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
            row[_metricColumn] = report.MetricVal;

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

        public Task SaveReportAsync(PluginSetupModel pluginSetup, IActionObserver observer)
        {
            return Task.Run(() => SaveReport(pluginSetup, observer));
        }

        private void SaveReport(PluginSetupModel pluginSetup, IActionObserver observer)
        {
            var dPlugin = pluginSetup.PluginRef.Metadata.Descriptor;
            var fileName = "Optimization of " + dPlugin.DisplayName + " " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + ".zip";
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
                            BacktesterReportViewModel.SaveAsText(entryStream, dPlugin.Type, rep.Stats);

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
                serializer.AddColumn(p.DisplayName, r => r.Config.Params[p.Id].ToString());

            serializer.AddColumn("Metric", r => r.Stats.FinalBalance);

            serializer.Serialize(reps, stream);
        }

        private void Clear()
        {
            _reports.Clear();
            Data.Clear();
            Data.Columns.Clear();
            _idToColumnMap.Clear();
        }
    }
}

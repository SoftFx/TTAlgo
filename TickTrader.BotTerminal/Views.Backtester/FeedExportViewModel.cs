using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class FeedExportViewModel : Screen, IWindowModel
    {
        private FeedCacheKey _key;
        private List<FeedExporter> _exporters = new List<FeedExporter>();
        private FeedExporter _selectedExporter;
        private TraderClientModel _client;
        private bool _isRangeLoaded;
        private bool _showDownloadUi;
        private CancellationTokenSource _cancelExportSrc;
        private Task _exportTask;

        public FeedExportViewModel(FeedCacheKey key, TraderClientModel clientModel)
        {
            _key = key;
            _client = clientModel;

            clientModel.Connected += UpdateState;
            clientModel.Disconnected += UpdateState;

            _exporters.Add(new CsvExporter());
            
            DateRange = new DateRangeSelectionViewModel();
            ExportObserver = new ProgressViewModel();

            SelectedExporter = _exporters[0];
            UpdateAvailableRange();
        }

        public IEnumerable<FeedExporter> Exporters => _exporters;
        public DateRangeSelectionViewModel DateRange { get; }
        public ProgressViewModel ExportObserver { get; }
        public bool CanExport { get; private set; }
        public bool CanCancel { get; private set; }
        public bool IsExporting => _cancelExportSrc != null;
        public bool IsCancelling => _cancelExportSrc?.IsCancellationRequested ?? false;
        public bool IsInputEnabled => !IsExporting && IsRangeLoaded;

        #region Observable Properties

        public bool ShowDownloadUi
        {
            get => _showDownloadUi;
            set
            {
                if (_showDownloadUi != value)
                {
                    _showDownloadUi = value;
                    NotifyOfPropertyChange(nameof(ShowDownloadUi));
                }
            }
        }

        public bool IsRangeLoaded
        {
            get => _isRangeLoaded;
            set
            {
                if (_isRangeLoaded != value)
                {
                    _isRangeLoaded = value;
                    NotifyOfPropertyChange(nameof(IsRangeLoaded));
                    UpdateState();
                }
            }
        }

        public FeedExporter SelectedExporter
        {
            get => _selectedExporter;
            set
            {
                if (_selectedExporter != value)
                {
                    if (SelectedExporter != null)
                        _selectedExporter.PropertyChanged -= _selectedExporter_PropertyChanged;
                    _selectedExporter = value;
                    _selectedExporter.PropertyChanged += _selectedExporter_PropertyChanged;
                    NotifyOfPropertyChange(nameof(SelectedExporter));
                    UpdateState();
                }
            }
        }

        private void _selectedExporter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateState();
        }

        #endregion Observable Properties

        public void Export()
        {
            _exportTask = DoExport();
        }

        public async void Cancel()
        {
            if (IsExporting)
            {
                _cancelExportSrc.Cancel();
                UpdateState();
                await _exportTask;
            }
            else
                TryClose();
        }

        public void Dispose()
        {
            _client.Connected -= UpdateState;
            _client.Disconnected -= UpdateState;
        }

        private void UpdateState()
        {
            CanExport = _client.IsConnected && DateRange.From != null && DateRange.To != null && !IsExporting && SelectedExporter.CanExport;
            CanCancel = !IsCancelling;
            NotifyOfPropertyChange(nameof(CanExport));
            NotifyOfPropertyChange(nameof(CanCancel));
            NotifyOfPropertyChange(nameof(IsInputEnabled));
        }

        private async Task DoExport()
        {
            _cancelExportSrc = new CancellationTokenSource();
            ShowDownloadUi = true;
            UpdateState();

            try
            {
                var from = DateRange.From.Value;
                var to = DateRange.To.Value;

                await Task.Factory.StartNew(() =>
                {
                    var cache = _client.History;

                    ExportObserver.StartProgress(from.TotalDays(), to.TotalDays());

                    var exporter = SelectedExporter;

                    exporter.StartExport();

                    try
                    {
                        foreach (var slice in cache.OnlineCache.IterateBarCache(_key, from, to))
                        {
                            exporter.ExportSlice(slice.From, slice.To, slice.Content);
                            ExportObserver.SetProgress(slice.To.TotalDays());
                        }

                    }
                    finally
                    {
                        exporter.EndExport();
                    }

                    //_feedHistoryProvider.ReadCache(
                });
            }
            catch (Exception ex)
            {
                ExportObserver.SetMessage(0, "Error:" + ex.Message);
            }
            finally
            {
                _cancelExportSrc = null;
                UpdateState();
            }

            TryClose();
        }

        private async void UpdateAvailableRange()
        {
            IsRangeLoaded = false;
            DateRange.From = null;
            DateRange.To = null;

            var range = await _client.History.GetCachedRange(_key, false);

            DateRange.UpdateBoundaries(range.Item1.Date, range.Item2.Date);
            IsRangeLoaded = true;
        }
    }

    internal abstract class FeedExporter : ObservableObject
    {
        private bool _valid;

        public FeedExporter(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public FeedCacheKey CollectionKey { get; private set; }

        public bool CanExport
        {
            get => _valid;
            set
            {
                if (_valid != value)
                {
                    _valid = value;
                    NotifyOfPropertyChange(nameof(CanExport));
                }
            }
        }

        public virtual void StartExport() { }
        public abstract void ExportSlice(DateTime from, DateTime to, ArraySegment<BarEntity> values);
        public virtual void EndExport() { }

        public virtual void Init(FeedCacheKey key)
        {
            CollectionKey = key;
        }
    }

    internal class CsvExporter : FeedExporter
    {
        private StreamWriter _writer;

        public CsvExporter() : base("CSV")
        {
            FileInput = new PathInputViewModel(PathInputModes.SaveFile) { Filter = "CSV file|*.csv" };

            FileInput.PropertyChanged += (s, a) =>
            {
                CanExport = FileInput.IsValid;
            };
        }

        public PathInputViewModel FileInput { get; }

        public override void StartExport()
        {
            _writer = new StreamWriter(File.Open(FileInput.Path, FileMode.Create));
        }

        public override void ExportSlice(DateTime from, DateTime to, ArraySegment<BarEntity> values)
        {
            foreach (var val in values)
            {
                _writer.Write(val.OpenTime);
                _writer.Write(",");
                _writer.Write(val.Open);
                _writer.Write(",");
                _writer.Write(val.High);
                _writer.Write(",");
                _writer.Write(val.Low);
                _writer.Write(",");
                _writer.Write(val.Close);
                _writer.Write(",");
                _writer.WriteLine(val.Volume);
            }
        }

        public override void EndExport()
        {
            try
            {
                _writer.Close();
            }
            catch (Exception) { }
        }
    }
}

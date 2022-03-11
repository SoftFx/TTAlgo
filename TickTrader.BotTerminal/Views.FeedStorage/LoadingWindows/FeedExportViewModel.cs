using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedExportViewModel : BaseLoadingWindow, IExportSeriesSettings
    {
        private readonly ISymbolData _symbol;
        private readonly IStorageSeries _series;


        public IEnumerable<SeriesFileExtensionsOptions> FileFormats { get; } = EnumHelper.AllValues<SeriesFileExtensionsOptions>();

        public Property<SeriesFileExtensionsOptions> SelectedFormat { get; }

        public Property<string> SelectedFolder { get; }

        public Property<string> FileName { get; }

        public Property<string> FileFilter { get; }


        public FeedExportViewModel(ISymbolData symbol, ISeriesKey key) : base($"Export Series: {key.FullInfo})")
        {
            _symbol = symbol;
            _series = symbol.Series[key];

            FileName = _varContext.AddProperty($"{key.FullInfo}.csv");
            FileFilter = _varContext.AddProperty<string>();
            SelectedFolder = _varContext.AddProperty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SelectedFormat = _varContext.AddProperty(FileFormats.First()).AddPostTrigger(UpdateFileFilter);

            UpdateAvailableRange(_symbol, key.TimeFrame, key.MarketSide);
        }


        private void UpdateFileFilter(SeriesFileExtensionsOptions newOptions)
        {
            switch (newOptions)
            {
                case SeriesFileExtensionsOptions.Csv:
                    FileFilter.Value = PackageHelper.CsvExtensions;
                    break;
                case SeriesFileExtensionsOptions.Txt:
                    FileFilter.Value = PackageHelper.TxtExtensions;
                    break;
            }
        }

        public void Export()
        {
            Task ExportAsync(IActionObserver observer)
            {
                return _series.ExportSeriesWithObserver(observer, this);
            }

            ShowProgressUi.Value = true;
            ProgressObserver.Start(ExportAsync);
        }


        char IExportSeriesSettings.Separator { get; } = ',';

        string IExportSeriesSettings.TimeFormat { get; } = "yyyy-MM-dd HH:mm:ss.fff";

        DateTime IExportSeriesSettings.From => DateRange.From;

        DateTime IExportSeriesSettings.To => DateRange.To + TimeSpan.FromDays(1);

        SeriesFileExtensionsOptions IExportSeriesSettings.FileType => SelectedFormat.Value;

        string IExportSeriesSettings.FilePath => Path.Combine(SelectedFolder.Value, FileName.Value);


        //private async Task DoExport()
        //{
        //    //    _cancelExportSrc = new CancellationTokenSource();
        //    //    ShowProgressUi.Value = true;
        //    //    UpdateState();

        //    //    try
        //    //    {
        //    //        var from = DateRange.From;
        //    //        var to = DateRange.To + TimeSpan.FromDays(1);

        //    //        await Actor.Spawn(async () =>
        //    //        {
        //    //            ProgressObserver.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

        //    //            var exporter = SelectedExporter;

        //    //            exporter.StartExport();

        //    //            try
        //    //            {
        //    //                if (!_series.Key.TimeFrame.IsTicks())
        //    //                {
        //    //                    var i = _series.IterateBarCache(from, to);

        //    //                    while (await i.ReadNext())
        //    //                    {
        //    //                        var slice = i.Current;
        //    //                        exporter.ExportSlice(slice.From, slice.To, slice.Content);
        //    //                        ProgressObserver.SetProgress(slice.To.GetAbsoluteDay());
        //    //                    }
        //    //                }
        //    //                else
        //    //                {
        //    //                    var i = _series.IterateTickCache(from, to);

        //    //                    while (await i.ReadNext())
        //    //                    {
        //    //                        var slice = i.Current;
        //    //                        exporter.ExportSlice(slice.From, slice.To, slice.Content);
        //    //                        ProgressObserver.SetProgress(slice.To.GetAbsoluteDay());
        //    //                    }
        //    //                }
        //    //            }
        //    //            finally
        //    //            {
        //    //                exporter.EndExport();
        //    //            }
        //    //        });
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        ProgressObserver.SetMessage("Error:" + ex.Message);
        //    //    }
        //    //    finally
        //    //    {
        //    //        _cancelExportSrc = null;
        //    //        UpdateState();
        //    //    }

        //    //    await TryCloseAsync();
        //}
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
        public abstract void ExportSlice(DateTime from, DateTime to, ArraySegment<BarData> values);
        public abstract void ExportSlice(DateTime from, DateTime to, ArraySegment<QuoteInfo> values);
        public virtual void EndExport() { }

        public virtual void Init(FeedCacheKey key)
        {
            CollectionKey = key;
        }
    }

    internal class CsvExporter : FeedExporter
    {
        private StreamWriter _writer;
        private string _path;

        public CsvExporter() : base("CSV")
        {
        }

        public string FilePath
        {
            get => _path;
            set
            {
                _path = value;
                CanExport = !string.IsNullOrWhiteSpace(_path);
            }
        }

        private string GetCorrectPath()
        {
            if (!Path.HasExtension(FilePath))
                return FilePath + ".csv";
            return FilePath;
        }

        public override void StartExport()
        {
            _writer = new StreamWriter(File.Open(GetCorrectPath(), FileMode.Create));
        }

        public override void ExportSlice(DateTime from, DateTime to, ArraySegment<BarData> values)
        {
            foreach (var val in values)
            {
                _writer.Write(val.OpenTime.ToUtcDateTime());
                _writer.Write(",");
                _writer.Write(val.Open);
                _writer.Write(",");
                _writer.Write(val.High);
                _writer.Write(",");
                _writer.Write(val.Low);
                _writer.Write(",");
                _writer.Write(val.Close);
                _writer.Write(",");
                _writer.WriteLine(val.RealVolume);
            }
        }

        public override void ExportSlice(DateTime from, DateTime to, ArraySegment<QuoteInfo> values)
        {
            foreach (var val in values)
            {
                _writer.Write(val.TimeUtc.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                var bids = val.L2Data.Bids;
                var asks = val.L2Data.Asks;

                for (int i = 0; i < Math.Max(bids.Length, asks.Length); i++)
                {
                    if (i < bids.Length)
                    {
                        _writer.Write(",");
                        _writer.Write(bids[i].Price);
                        _writer.Write(",");
                        _writer.Write(bids[i].Amount);
                    }
                    else
                        _writer.Write(",,");

                    if (i < asks.Length)
                    {
                        _writer.Write(",");
                        _writer.Write(asks[i].Price);
                        _writer.Write(",");
                        _writer.Write(asks[i].Amount);
                    }
                    else
                        _writer.Write(",,");
                }

                _writer.WriteLine();
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
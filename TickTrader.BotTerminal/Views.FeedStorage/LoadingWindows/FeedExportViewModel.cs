using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedExportViewModel : BaseLoadingWindow, IExportSeriesSettings
    {
        private readonly IStorageSeries _series;


        public IEnumerable<SeriesFileExtensionsOptions> FileFormats { get; } = EnumHelper.AllValues<SeriesFileExtensionsOptions>();

        public Property<SeriesFileExtensionsOptions> SelectedFormat { get; }

        public Property<string> SelectedFolder { get; }

        public Property<string> FileName { get; }

        public Property<string> FileFilter { get; }


        public FeedExportViewModel(IStorageSeries series) : base($"Export Series: {series.Key.FullInfo})")
        {
            _series = series;

            FileName = _varContext.AddProperty(series.Key.FullInfo);
            FileFilter = _varContext.AddProperty<string>();
            SelectedFolder = _varContext.AddProperty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SelectedFormat = _varContext.AddProperty(FileFormats.First()).AddPostTrigger(UpdateFileFilter);

            UpdateAvailableRange((_series.From, _series.To));
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

            FileName.Value = Path.ChangeExtension(FileName.Value, $".{newOptions.ToString().ToLowerInvariant()}");
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

        DateTime IExportSeriesSettings.To => DateRange.To + TimeSpan.FromMilliseconds(1);

        SeriesFileExtensionsOptions IExportSeriesSettings.FileType => SelectedFormat.Value;

        string IExportSeriesSettings.FilePath => Path.Combine(SelectedFolder.Value, FileName.Value);
    }
}
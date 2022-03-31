using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FileViewManager : IExportSeriesSettings, IImportSeriesSettings
    {
        private readonly DateRangeSelectionViewModel _range;
        private readonly VarContext _varContext;


        public IEnumerable<SeriesFileExtensionsOptions> FileFormats { get; } = EnumHelper.AllValues<SeriesFileExtensionsOptions>();

        public Property<SeriesFileExtensionsOptions> SelectedFormat { get; }

        public Property<string> SelectedFolder { get; }

        public Property<string> FileFilter { get; }

        public Validable<string> FileName { get; }

        public BoolProperty SkipHeader { get; }

        public BoolVar Ready { get; }


        SeriesFileExtensionsOptions IBaseFileSeriesSettings.FileType => SelectedFormat.Value;

        string IBaseFileSeriesSettings.FilePath => Path.Combine(SelectedFolder.Value, FileName.Value);

        char IBaseFileSeriesSettings.Separator { get; } = ',';

        string IBaseFileSeriesSettings.TimeFormat { get; } = "yyyy-MM-dd HH:mm:ss.fff";


        DateTime IExportSeriesSettings.From => _range.From;

        DateTime IExportSeriesSettings.To => _range.To + TimeSpan.FromMilliseconds(1);

        bool IImportSeriesSettings.SkipHeaderLine => SkipHeader.Value;


        internal FileViewManager(VarContext context, DateRangeSelectionViewModel range)
        {
            _varContext = context;
            _range = range;

            FileName = _varContext.AddValidable<string>().MustBeNotEmpty();

            SkipHeader = _varContext.AddBoolProperty();
            FileFilter = _varContext.AddProperty<string>();
            SelectedFolder = _varContext.AddProperty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SelectedFormat = _varContext.AddProperty(FileFormats.First()).AddPostTrigger(UpdateFileFilter);

            Ready = FileName.IsValid();
        }


        private void UpdateFileFilter(SeriesFileExtensionsOptions newOptions)
        {
            switch (newOptions)
            {
                case SeriesFileExtensionsOptions.Csv:
                    FileFilter.Value = PackageHelper.CsvExtensions;
                    SkipHeader.Value = true;
                    break;
                case SeriesFileExtensionsOptions.Txt:
                    FileFilter.Value = PackageHelper.TxtExtensions;
                    SkipHeader.Value = false;
                    break;
            }

            FileName.Value = Path.ChangeExtension(FileName.Value, $".{newOptions.ToString().ToLowerInvariant()}");
        }
    }
}

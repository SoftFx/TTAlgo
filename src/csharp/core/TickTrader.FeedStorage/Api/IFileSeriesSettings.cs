using System;

namespace TickTrader.FeedStorage.Api
{
    public enum SeriesFileExtensionsOptions
    {
        Csv,
        Txt,
    }


    public interface IBaseFileSeriesSettings
    {
        SeriesFileExtensionsOptions FileType { get; }

        string FilePath { get; }

        char Separator { get; }

        string TimeFormat { get; }
    }


    public interface IExportSeriesSettings : IBaseFileSeriesSettings
    {
        DateTime From { get; }

        DateTime To { get; }
    }


    public interface IImportSeriesSettings : IBaseFileSeriesSettings
    {
        bool SkipHeaderLine { get; }
    }
}

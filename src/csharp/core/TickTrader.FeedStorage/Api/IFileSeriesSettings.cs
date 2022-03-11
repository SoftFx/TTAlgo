using System;

namespace TickTrader.FeedStorage.Api
{
    public enum SeriesFileExtensionsOptions
    {
        Csv,
        Txt,
    }

    public interface IExportSeriesSettings
    {
        SeriesFileExtensionsOptions FileType { get; }

        string FilePath { get; }

        char Separator { get; }

        string TimeFormat { get; }

        DateTime From { get; }

        DateTime To { get; }
    }

    public interface IImportSeriesSettings
    {
    }
}

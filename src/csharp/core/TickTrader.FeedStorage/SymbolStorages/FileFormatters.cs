using System.IO;

namespace TickTrader.FeedStorage.StorageBase
{
    internal abstract class BaseFileFormatter
    {
        internal virtual void PreloadLogic(StreamWriter writer) { }

        internal virtual void PostloadLogic(StreamWriter writer) { }
    }


    internal sealed class CsvFileFormatter : BaseFileFormatter
    {
        internal override void PostloadLogic(StreamWriter writer)
        {
        }
    }


    internal sealed class TxtFileFormatter : BaseFileFormatter
    {
    }
}

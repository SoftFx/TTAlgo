using System.IO;

namespace TickTrader.FeedStorage.StorageBase
{
    internal abstract class BaseFileFormatter
    {
        internal char Separator { get; set; }


        internal virtual void WriteBarFileHeader(StreamWriter writer) { }

        internal virtual void WriteTickFileHeader(StreamWriter writer) { }

        internal virtual void WriteTickL2FileHeader(StreamWriter writer) { }


        internal virtual void WriteFileTail(StreamWriter writer) { }
    }


    internal sealed class CsvFileFormatter : BaseFileFormatter
    {
        internal override void WriteBarFileHeader(StreamWriter writer)
        {
            var header = new string[] { "Time", "Open", "High", "Low", "Close", "Volume" };

            WriteHeader(writer, header);
        }

        internal override void WriteTickFileHeader(StreamWriter writer)
        {
            var header = new string[] { "Time", "BidPrice", "AskPrice" };

            WriteHeader(writer, header);
        }

        internal override void WriteTickL2FileHeader(StreamWriter writer)
        {
            var header = new string[] { "Time", "BidPrice", "BidVolume", "AskPrice", "AskVolume" };

            WriteHeader(writer, header);
        }


        private void WriteHeader(StreamWriter writer, string[] header)
        {
            writer.WriteLine(string.Join(Separator.ToString(), header));
        }
    }


    internal sealed class TxtFileFormatter : BaseFileFormatter
    {
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal sealed class TickFileHandler : BaseFileHandler<QuoteInfo>
    {
        private readonly string _doubleSeparator;

        public TickFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, FeedCacheKey key, IExportSeriesSettings settings) : base(storage, formatter, key, settings)
        {
            _doubleSeparator = $"{_separator}{_separator}";
        }

        protected override void PreloadLogic(StreamWriter writer)
        {
            _formatter.WriteTickFileHeader(writer);
        }

        protected override void PostloadLogic(StreamWriter writer) { }

        protected override void WriteSlice(ArraySegment<QuoteInfo> values)
        {
            if (_writer == null)
                return;

            foreach (var val in values)
            {
                _writer.Write(val.TimeUtc.ToString(_timeFormat));

                var bids = val.L2Data.Bids;
                var asks = val.L2Data.Asks;

                for (int i = 0; i < Math.Max(bids.Length, asks.Length); i++)
                {
                    WriteBand(i, bids);
                    WriteBand(i, asks);
                }

                _writer.WriteLine();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBand(int index, ReadOnlySpan<QuoteBand> band)
        {
            if (index < band.Length)
            {
                _writer.Write(_separator);
                _writer.Write(band[index].Price);
                _writer.Write(_separator);
                _writer.Write(band[index].Amount);
            }
            else
                _writer.Write(_doubleSeparator);
        }
    }
}

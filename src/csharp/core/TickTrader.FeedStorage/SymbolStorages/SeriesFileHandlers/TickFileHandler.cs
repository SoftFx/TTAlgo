using ActorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal sealed class TickFileHandler : BaseFileHandler<QuoteInfo>
    {
        private const int StackSize = 1 << 8;

        private readonly string _doubleSeparator;

        protected override ICollection<QuoteInfo> Vector { get; }


        public TickFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, FeedCacheKey key, IBaseFileSeriesSettings settings) : base(storage, formatter, key, settings)
        {
            _doubleSeparator = $"{_separator}{_separator}";

            Vector = new List<QuoteInfo>();
        }


        protected override void PreloadLogic(StreamWriter writer)
        {
            _formatter.WriteTickFileHeader(writer);
        }

        protected override void PostloadLogic(StreamWriter writer) { }


        protected override void WriteSliceToStream(ArraySegment<QuoteInfo> values)
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

        protected override QuoteInfo ReadSlice(string line, int lineNumber)
        {
            var parts = line.Split(_separator);
            var maxDepth = (parts.Length - 1) % 4;

            if (parts.Length < 5 || maxDepth != 0)
                ThrowFormatError(lineNumber);

            var asks = maxDepth > StackSize
                ? new QuoteBand[maxDepth]
                : stackalloc QuoteBand[maxDepth];

            var bids = maxDepth > StackSize
                ? new QuoteBand[maxDepth]
                : stackalloc QuoteBand[maxDepth];

            var time = DateTime.Parse(parts[0]);

            var bidDepth = 0;
            var askDepth = 0;
            var partsSpan = parts.AsSpan(1);
            for (var i = 0; i < maxDepth; i++)
            {
                // partsSpan[2] and partsSpan[3] should both have empty strings if band is not present
                // checking partsSpan[3] should eliminate further bound checks
                if (!string.IsNullOrEmpty(partsSpan[3]))
                    asks[askDepth++] = new QuoteBand(double.Parse(partsSpan[2]), double.Parse(partsSpan[3]));
                if (!string.IsNullOrEmpty(partsSpan[1]))
                    bids[bidDepth++] = new QuoteBand(double.Parse(partsSpan[0]), double.Parse(partsSpan[1]));
            }

            var data = new QuoteData
            {
                UtcTicks = time.ToUniversalTime().Ticks,
                IsBidIndicative = false,
                IsAskIndicative = false,
                AskBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(asks.Slice(askDepth))),
                BidBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(bids.Slice(bidDepth))),
            };

            return QuoteInfo.Create(string.Empty, data);
        }

        protected override async Task WritePageToStorage(ActorChannel<ISliceInfo> buffer, QuoteInfo[] values)
        {
            var from = values[0].TimeUtc;
            var to = values[values.Length - 1].TimeUtc.AddTicks(1);

            _storage.Put(_key, from, to, values);

            if (!await buffer.Write(new SliceInfo(from, to, values.Length)))
                throw new TaskCanceledException();
        }
    }
}
using ActorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal sealed class BarFileHandler : BaseFileHandler<BarData>
    {
        protected override ICollection<BarData> Vector { get; }


        public BarFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, FeedCacheKey key, IBaseFileSeriesSettings settings) : base(storage, formatter, key, settings)
        {
            Vector = new BarVector(key.TimeFrame);
        }


        protected override void PreloadLogic(StreamWriter writer)
        {
            _formatter.WriteBarFileHeader(writer);
        }

        protected override void PostloadLogic(StreamWriter writer) { }


        protected override void WriteSliceToStream(ArraySegment<BarData> values)
        {
            if (_writer == null)
                return;

            foreach (var val in values)
            {
                _writer.Write(val.OpenTime.ToUtcDateTime().ToString(_timeFormat));
                _writer.Write(_separator);
                _writer.Write(val.Open);
                _writer.Write(_separator);
                _writer.Write(val.High);
                _writer.Write(_separator);
                _writer.Write(val.Low);
                _writer.Write(_separator);
                _writer.Write(val.Close);
                _writer.Write(_separator);
                _writer.WriteLine(val.RealVolume);
            }
        }

        protected override BarData ReadSlice(string line, int lineNumber)
        {
            var parts = line.Split(_separator);

            if (parts.Length != 6)
                ThrowFormatError(lineNumber);

            var time = new UtcTicks(ParseDate(parts[0]).ToUniversalTime().Ticks);

            return new BarData(time, time) // need a timeframe to init bars correctly
            {
                Open = double.Parse(parts[1]),
                High = double.Parse(parts[2]),
                Low = double.Parse(parts[3]),
                Close = double.Parse(parts[4]),
                RealVolume = double.Parse(parts[5])
            };
        }

        protected override async Task WritePageToStorage(ActorChannel<ISliceInfo> buffer, BarData[] values)
        {
            var from = values[0].OpenTime.ToUtcDateTime();
            var to = values[values.Length - 1].CloseTime.ToUtcDateTime();

            _storage.Put(_key, from, to, values);

            if (!await buffer.Write(new SliceInfo(from, to, values.Length)))
                throw new TaskCanceledException();
        }
    }
}
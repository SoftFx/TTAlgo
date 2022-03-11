using System;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal sealed class BarFileHandler : BaseFileHandler<BarData>
    {
        public BarFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, IExportSeriesSettings settings) : base(storage, formatter, settings)
        {
        }

        protected override void WriteSlice(ArraySegment<BarData> values)
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
    }

}

using System;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal sealed class TickFileHandler : BaseFileHandler<QuoteInfo>
    {
        private readonly string _doubleSeparator;


        public TickFileHandler(FeedStorageBase storage, BaseFileFormatter formatter, IExportSeriesSettings settings) : base(storage, formatter, settings)
        {
            _doubleSeparator = $"{_separator}{_separator}";
        }


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
                    if (i < bids.Length)
                    {
                        _writer.Write(_separator);
                        _writer.Write(bids[i].Price);
                        _writer.Write(_separator);
                        _writer.Write(bids[i].Amount);
                    }
                    else
                        _writer.Write(_doubleSeparator);

                    if (i < asks.Length)
                    {
                        _writer.Write(_separator);
                        _writer.Write(asks[i].Price);
                        _writer.Write(_separator);
                        _writer.Write(asks[i].Amount);
                    }
                    else
                        _writer.Write(_doubleSeparator);
                }

                _writer.WriteLine();
            }
        }
    }
}

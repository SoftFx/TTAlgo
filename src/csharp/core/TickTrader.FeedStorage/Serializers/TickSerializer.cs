using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LightSerializer;

namespace TickTrader.FeedStorage.Serializers
{
    public static class TickSerializer
    {
        public static ISliceSerializer<QuoteInfo> GetSerializer(FeedCacheKey key)
        {
            if (key.TimeFrame == Feed.Types.Timeframe.Ticks)
                return new TopOfTheBook(key.Symbol);
            else if (key.TimeFrame == Feed.Types.Timeframe.TicksLevel2)
                return new FullBook(key.Symbol);

            throw new ArgumentException("Time frame is not supported: " + key.TimeFrame);
        }

        public class TopOfTheBook : ISliceSerializer<QuoteInfo>
        {
            private string _symbol;
            private LightObjectReader _reader = new LightObjectReader();

            [Flags]
            private enum FieldFlags : byte { Empty = 0, HasBid = 1, HasAsk = 2 }

            public TopOfTheBook(string symbolName)
            {
                _symbol = symbolName;
            }

            public QuoteInfo[] Deserialize(ArraySegment<byte> bytes)
            {
                _reader.SetDataBuffer(bytes);
                return _reader.ReadArray(ReadQuote);
            }

            private QuoteInfo ReadQuote(LightObjectReader reader)
            {
                var time = reader.ReadUtcTicks();

                double? bid = null, ask = null;
                var flags = (FieldFlags)reader.ReadByte();
                if ((flags & FieldFlags.HasBid) != 0)
                    bid = reader.ReadDouble();
                if ((flags & FieldFlags.HasAsk) != 0)
                    ask = reader.ReadDouble();

                return new QuoteInfo(_symbol, time, bid, ask)
                {
                    IsBidIndicative = false,
                    IsAskIndicative = false,
                };
            }

            public ArraySegment<byte> Serialize(QuoteInfo[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, WriteQuote);
                return writer.GetBuffer();
            }

            private static void WriteQuote(QuoteInfo quote, LightObjectWriter writer)
            {
                writer.Write(quote.UtcTicks);

                var flags = quote.HasBid ? FieldFlags.HasBid : FieldFlags.Empty;
                flags |= quote.HasAsk ? FieldFlags.HasAsk : FieldFlags.Empty;

                writer.Write((byte)flags);

                if (quote.HasBid)
                    writer.Write(quote.Bid);

                if (quote.HasAsk)
                    writer.Write(quote.Ask);
            }
        }

        public class FullBook : ISliceSerializer<QuoteInfo>
        {
            private string _symbol;
            private LightObjectReader _reader = new LightObjectReader();

            public FullBook(string symbolName)
            {
                _symbol = symbolName;
            }

            public QuoteInfo[] Deserialize(ArraySegment<byte> bytes)
            {
                _reader.SetDataBuffer(bytes);
                return _reader.ReadArray(ReadQuote);
            }

            private QuoteInfo ReadQuote(LightObjectReader reader)
            {
                var time = reader.ReadUtcTicks();
                var bids = ReadBook(reader);
                var asks = ReadBook(reader);

                return new QuoteInfo(_symbol, time, bids, asks)
                {
                    IsBidIndicative = false,
                    IsAskIndicative = false,
                };
            }

            private static byte[] ReadBook(LightObjectReader reader)
            {
                var cnt = reader.ReadInt();
                var bytes = new byte[QuoteBand.Size * cnt];

                reader.ReadBytes(bytes);

                return bytes;
            }

            public ArraySegment<byte> Serialize(QuoteInfo[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, (e, w) =>
                {
                    writer.Write(e.UtcTicks);

                    writer.Write(e.BidBytes.Length / QuoteBand.Size);
                    writer.Write(e.BidBytes);

                    writer.Write(e.AskBytes.Length / QuoteBand.Size);
                    writer.Write(e.AskBytes);
                });
                return writer.GetBuffer();
            }
        }
    }
}

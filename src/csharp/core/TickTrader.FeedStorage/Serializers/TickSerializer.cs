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
                var time = reader.ReadDateTime(DateTimeKind.Utc).ToTimestamp();

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

            private static ByteString ReadBook(LightObjectReader reader)
            {
                Span<QuoteBand> bands = stackalloc QuoteBand[] { new QuoteBand(reader.ReadDouble(), 0) };
                return ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(bands));
            }

            public ArraySegment<byte> Serialize(QuoteInfo[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, WriteQuote);
                return writer.GetBuffer();
            }

            private static void WriteQuote(QuoteInfo quote, LightObjectWriter writer)
            {
                writer.Write(quote.Time);

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
                var time = reader.ReadDateTime(DateTimeKind.Utc).ToTimestamp();
                var bids = ReadBook(reader);
                var asks = ReadBook(reader);

                return new QuoteInfo(_symbol, time, bids, asks)
                {
                    IsBidIndicative = false,
                    IsAskIndicative = false,
                };
            }

            private static QuoteBand[] ReadBook(LightObjectReader reader)
            {
                var cnt = reader.ReadInt();
                var bands = new QuoteBand[cnt];

                var bytes = MemoryMarshal.Cast<QuoteBand, byte>(bands);
                reader.ReadBytes(bytes);

                return bands;
            }

            public ArraySegment<byte> Serialize(QuoteInfo[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, (e, w) =>
                {
                    writer.Write(e.Time);

                    writer.Write(e.Bids.Length);
                    writer.Write(MemoryMarshal.Cast<QuoteBand, byte>(e.Bids));

                    writer.Write(e.Asks.Length);
                    writer.Write(MemoryMarshal.Cast<QuoteBand, byte>(e.Asks));
                });
                return writer.GetBuffer();
            }
        }
    }
}

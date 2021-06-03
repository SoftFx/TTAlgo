using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LightSerializer;

namespace TickTrader.FeedStorage
{
    internal static class TickSerializer
    {
        public static ISliceSerializer<QuoteInfo> GetSerializer(FeedCacheKey key)
        {
            if (key.Frame == Feed.Types.Timeframe.Ticks)
                return new TopOfTheBook(key.Symbol);
            else if (key.Frame == Feed.Types.Timeframe.TicksLevel2)
                return new FullBook(key.Symbol);

            throw new ArgumentException("Time frame is not supported: " + key.Frame);
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
                return new QuoteInfo(_symbol, ReadData(reader));
            }

            private static QuoteData ReadData(LightObjectReader reader)
            {
                var data = new QuoteData
                {
                    Time = reader.ReadDateTime(DateTimeKind.Utc).ToTimestamp(),
                    IsBidIndicative = false,
                    IsAskIndicative = false
                };

                var flags = (FieldFlags)reader.ReadByte();
                if ((flags & FieldFlags.HasBid) != 0)
                    data.BidBytes = ReadBook(reader);
                if ((flags & FieldFlags.HasAsk) != 0)
                    data.AskBytes = ReadBook(reader);

                return data;
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
                    WriteBook(quote.Bids, writer);

                if (quote.HasAsk)
                    WriteBook(quote.Asks, writer);

            }

            private static void WriteBook(ReadOnlySpan<QuoteBand> bands, LightObjectWriter writer)
            {
                writer.Write(bands[0].Price);
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
                return new QuoteInfo(_symbol, ReadData(reader));
            }

            private static QuoteData ReadData(LightObjectReader reader)
            {
                var data = new QuoteData
                {
                    Time = reader.ReadDateTime(DateTimeKind.Utc).ToTimestamp(),
                    IsBidIndicative = false,
                    IsAskIndicative = false,
                };

                data.BidBytes = ReadBook(reader);
                data.AskBytes = ReadBook(reader);

                return data;
            }

            private static ByteString ReadBook(LightObjectReader reader)
            {
                var cnt = reader.ReadInt();
                var bands = cnt > 256
                    ? new QuoteBand[cnt].AsSpan()
                    : stackalloc QuoteBand[cnt];

                var bytes = MemoryMarshal.Cast<QuoteBand, byte>(bands);
                reader.ReadBytes(bytes);

                return ByteStringHelper.CopyFromUglyHack(bytes);
            }

            public ArraySegment<byte> Serialize(QuoteInfo[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, (e, w) =>
                {
                    writer.Write(e.Time);

                    writer.Write(e.Bids.Length);
                    writer.Write(e.BidBytes);

                    writer.Write(e.Asks.Length);
                    writer.Write(e.AskBytes);
                });
                return writer.GetBuffer();
            }
        }
    }
}

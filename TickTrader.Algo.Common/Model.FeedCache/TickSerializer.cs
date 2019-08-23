using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LightSerializer;

namespace TickTrader.Algo.Common.Model
{
    internal static class TickSerializer
    {
        public static ISliceSerializer<QuoteEntity> GetSerializer(FeedCacheKey key)
        {
            if (key.Frame == Api.TimeFrames.Ticks)
                return new TopOfTheBook(key.Symbol);
            else if(key.Frame == Api.TimeFrames.TicksLevel2)
                return new FullBook(key.Symbol);

            throw new ArgumentException("Time frame is not supported: " + key.Frame);
        }

        public class TopOfTheBook : ISliceSerializer<QuoteEntity>
        {
            private string _symbol;
            private LightObjectReader _reader = new LightObjectReader();

            [Flags]
            private enum FieldFlags : byte { Empty = 0, HasBid = 1, HasAsk = 2 }

            public TopOfTheBook(string symbolName)
            {
                _symbol = symbolName;
            }

            public QuoteEntity[] Deserialize(ArraySegment<byte> bytes)
            {
                _reader.SetDataBuffer(bytes);
                return _reader.ReadArray(ReadQuote);
            }

            private QuoteEntity ReadQuote(LightObjectReader reader)
            {
                var time = reader.ReadDateTime(DateTimeKind.Utc);
                var flags = (FieldFlags)reader.ReadByte();

                Api.BookEntry[] bids;
                Api.BookEntry[] asks;

                if ((flags & FieldFlags.HasBid) != 0)
                    bids = new Api.BookEntry[] { ReadBookEntry(reader) };
                else
                    bids = new Api.BookEntry[0];
                if ((flags & FieldFlags.HasAsk) != 0)
                    asks = new Api.BookEntry[] { ReadBookEntry(reader) };
                else
                    asks = new Api.BookEntry[0];

                return QuoteEntity.CreatePrepared(_symbol, time, bids, asks);
            }

            private Api.BookEntry ReadBookEntry(LightObjectReader reader)
            {
                //return new Api.BookEntry(reader.ReadDouble(), reader.ReadDouble());
                return new Api.BookEntry(reader.ReadDouble(), 0);
            }

            public ArraySegment<byte> Serialize(QuoteEntity[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, WriteQuote);
                return writer.GetBuffer();
            }

            private void WriteQuote(QuoteEntity quote, LightObjectWriter writer)
            {
                writer.Write(quote.Time);

                var flags = quote.HasBid ? FieldFlags.HasBid : FieldFlags.Empty;
                flags |= quote.HasAsk ? FieldFlags.HasAsk : FieldFlags.Empty;

                writer.Write((byte)flags);

                if (quote.HasBid)
                    WriteBookEntry(quote.BidBook[0], writer);

                if (quote.HasAsk)
                    WriteBookEntry(quote.AskBook[0], writer);

            }

            private void WriteBookEntry(Api.BookEntry entry, LightObjectWriter writer)
            {
                writer.Write(entry.Price);
                //writer.Write(entry.Volume);
            }
        }

        public class FullBook : ISliceSerializer<QuoteEntity>
        {
            private string _symbol;
            private LightObjectReader _reader = new LightObjectReader();

            public FullBook(string symbolName)
            {
                _symbol = symbolName;
            }

            public QuoteEntity[] Deserialize(ArraySegment<byte> bytes)
            {
                _reader.SetDataBuffer(bytes);
                return _reader.ReadArray(ReadQuote);
            }

            private QuoteEntity ReadQuote(LightObjectReader reader)
            {
                var time = reader.ReadDateTime(DateTimeKind.Utc);

                var bids = reader.ReadArray(ReadBookEntry);
                var asks = reader.ReadArray(ReadBookEntry);

                return QuoteEntity.CreatePrepared(_symbol, time, bids, asks);
            }

            private Api.BookEntry ReadBookEntry(LightObjectReader reader)
            {
                return new Api.BookEntry(reader.ReadDouble(), reader.ReadDouble());
            }

            public ArraySegment<byte> Serialize(QuoteEntity[] val)
            {
                var writer = new LightObjectWriter();
                writer.WriteFixedSizeArray(val, (e, w) =>
                {
                    w.Write(e.Time);

                    w.WriteFixedSizeArray(e.BidBook, (e1, w1) =>
                    {
                        w1.Write(e1.Price);
                        w1.Write(e1.Volume);
                    });
                    w.WriteFixedSizeArray(e.AskBook, (e1, w1) =>
                    {
                        w1.Write(e1.Price);
                        w1.Write(e1.Volume);
                    });
                });
                return writer.GetBuffer();
            }
        }
    }
}

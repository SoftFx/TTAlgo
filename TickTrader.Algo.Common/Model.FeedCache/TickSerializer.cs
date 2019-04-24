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
    internal class TickSerializer : ISliceSerializer<QuoteEntity>
    {
        private string _symbol;

        public TickSerializer(string symbolName)
        {
            _symbol = symbolName;
        }

        public QuoteEntity[] Deserialize(ArraySegment<byte> bytes)
        {
            var reader = new LightObjectReader(bytes);

            return reader.ReadFixedSizeArray(r =>
            {
                var time = reader.ReadDateTime(DateTimeKind.Utc);

                var bids = r.ReadFixedSizeArray(r1 =>
                {
                    return new BookEntryEntity
                    {
                        Price = r1.ReadDouble(),
                        Volume = r.ReadDouble()
                    };
                });

                var asks = r.ReadFixedSizeArray(r1 =>
                {
                    return new BookEntryEntity
                    {
                        Price = r1.ReadDouble(),
                        Volume = r.ReadDouble()
                    };
                });

                return new QuoteEntity(_symbol, time, bids, asks);
            });
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

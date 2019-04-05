using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LightSerializer;

namespace TickTrader.Algo.Common.Model
{
    internal class BarSerializer : ISliceSerializer<BarEntity>
    {
        public BarSampler _sampler;

        public BarSerializer(TimeFrames frames)
        {
            _sampler = BarSampler.Get(frames);
        }

        public ArraySegment<byte> Serialize(BarEntity[] val)
        {
            var writer = new LightObjectWriter();
            writer.WriteFixedSizeArray(val, (e, w) =>
            {
                w.Write(e.OpenTime);
                w.Write(e.Open);
                w.Write(e.High);
                w.Write(e.Low);
                w.Write(e.Close);
                w.Write(e.Volume);
            });
            return writer.GetBuffer();
        }

        public BarEntity[] Deserialize(ArraySegment<byte> bytes)
        {
            var reader = new LightObjectReader(bytes);
            return reader.ReadFixedSizeArray((r) =>
            {
                var time = r.ReadDateTime(DateTimeKind.Utc);
                var open = r.ReadDouble();
                var high = r.ReadDouble();
                var low = r.ReadDouble();
                var close = r.ReadDouble();
                var volume = r.ReadDouble();
                var boundaries = _sampler.GetBar(time);
                return new BarEntity { OpenTime = boundaries.Open, CloseTime = boundaries.Close, Close = close, High = high, Open = open, Low = low, Volume = volume };
            });
        }
    }
}

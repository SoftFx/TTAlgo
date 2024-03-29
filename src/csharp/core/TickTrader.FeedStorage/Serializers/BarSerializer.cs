﻿using System;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.LightSerializer;

namespace TickTrader.FeedStorage.Serializers
{
    public class BarSerializer : ISliceSerializer<BarData>
    {
        private readonly LightObjectReader _reader = new LightObjectReader();
        public readonly BarSampler _sampler;

        public BarSerializer(Feed.Types.Timeframe frames)
        {
            _sampler = BarSampler.Get(frames);
        }

        public ArraySegment<byte> Serialize(BarData[] val)
        {
            var writer = new LightObjectWriter();
            writer.WriteFixedSizeArray(val, (e, w) =>
            {
                w.Write(e.OpenTime.Value);
                w.Write(e.Open);
                w.Write(e.High);
                w.Write(e.Low);
                w.Write(e.Close);
                w.Write(e.RealVolume);
            });
            return writer.GetBuffer();
        }

        public BarData[] Deserialize(ArraySegment<byte> bytes)
        {
            _reader.SetDataBuffer(bytes);
            return _reader.ReadArray((r) =>
            {
                var time = new UtcTicks(r.ReadUtcTicks());
                var open = r.ReadDouble();
                var high = r.ReadDouble();
                var low = r.ReadDouble();
                var close = r.ReadDouble();
                var volume = r.ReadDouble();
                var boundaries = _sampler.GetBar(time);
                return new BarData(boundaries.Open, boundaries.Close) { Close = close, High = high, Open = open, Low = low, RealVolume = volume };
            });
        }
    }
}

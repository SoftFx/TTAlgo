﻿using System;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage.Serializers
{
    public sealed class DateTimeKeySerializer : IKeySerializer<DateTime>
    {
        public int KeySize => 8;

        public DateTime Deserialize(IKeyReader reader)
        {
            return new DateTime(reader.ReadBeLong(), DateTimeKind.Utc);
        }

        public void Serialize(DateTime key, IKeyBuilder builder)
        {
            builder.WriteBe(TimeSlicer.ToUtc(key).Ticks);
        }
    }
}

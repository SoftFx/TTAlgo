using System;

namespace TickTrader.SeriesStorage.Tests
{
    public class DateTimeKeySerializer : IKeySerializer<DateTime>
    {
        public int KeySize => 8;

        public DateTime Deserialize(IKeyReader reader)
        {
            var ticks = reader.ReadBeLong();
            return new DateTime(ticks);
        }

        public void Serialize(DateTime key, IKeyBuilder builder)
        {
            builder.WriteBe(key.Ticks);
        }
    }
}

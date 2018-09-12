using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest
{
    public class DatTimeKeySerializer : IKeySerializer<DateTime>
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

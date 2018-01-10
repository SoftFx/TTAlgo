using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.Common.Model
{
    internal class DateTimeKeySerializer : IKeySerializer<DateTime>
    {
        public int KeySize => 8;

        public DateTime Deserialize(IKeyReader reader)
        {
            return new DateTime(reader.ReadBeLong());
        }

        public void Serialize(DateTime key, IKeyBuilder builder)
        {
            builder.WriteBe(key.Ticks);
        }
    }
}

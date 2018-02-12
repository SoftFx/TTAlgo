using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.Common.Model
{
    public class GuidKeySerializer : IKeySerializer<Guid>
    {
        public int KeySize => 16;

        public Guid Deserialize(IKeyReader reader)
        {
            return new Guid(reader.ReadByteArray(16));
        }

        public void Serialize(Guid key, IKeyBuilder builder)
        {
            builder.Write(key.ToByteArray());
        }
    }
}

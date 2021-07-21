using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System.Linq;

namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class PluginConfig
    {
        public PluginConfig PackProperties(RepeatedField<Any> properties)
        {
            Properties.Clear();
            Properties.AddRange(properties.Select(p => Any.Pack(p)));

            return this;
        }
    }
}

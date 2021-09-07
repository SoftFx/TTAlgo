using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class PluginConfig
    {
        public PluginConfig PackProperties(IEnumerable<IPropertyConfig> properties)
        {
            Properties.Clear();
            Properties.AddRange(properties.Select(p => Any.Pack(p)));

            return this;
        }

        public PluginConfig PackProperties(RepeatedField<Any> properties)
        {
            Properties.Clear();
            Properties.AddRange(properties.Select(p => Any.Pack(p)));

            return this;
        }

        public List<IPropertyConfig> UnpackProperties()
        {
            var res = new List<IPropertyConfig>();
            foreach(var p in Properties)
            {
                if (PropertyConfig.TryUnpack(p, out var prop))
                    res.Add(prop);
            }
            return res;
        }
    }
}

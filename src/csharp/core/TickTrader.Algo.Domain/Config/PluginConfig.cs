using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class PluginConfig
    {
        public void PackProperties(IEnumerable<IPropertyConfig> properties)
        {
            Properties.Clear();
            Properties.AddRange(properties.Select(p => Any.Pack(p)));
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

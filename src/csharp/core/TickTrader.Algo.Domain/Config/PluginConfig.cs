using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class PluginConfig
    {
        private static readonly TypeRegistry _typeRegistry = TypeRegistry.FromFiles(RuntimePluginReflection.Descriptor);


        public static string BinaryUri { get; } = Descriptor.FullName;

        public static string JsonUri { get; } = Descriptor.FullName + "/Json";

        public static JsonParser JsonParser { get; } = new JsonParser(new JsonParser.Settings(16, _typeRegistry));

        public static JsonFormatter JsonFormatter { get; } = new JsonFormatter(new JsonFormatter.Settings(true, _typeRegistry));


        public PluginConfig PackProperties(IEnumerable<IPropertyConfig> properties)
        {
            Properties.Clear();
            Properties.AddRange(properties.Select(p => Any.Pack(p, "ttalgo")));

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

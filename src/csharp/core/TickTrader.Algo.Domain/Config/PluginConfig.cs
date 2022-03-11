using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class PluginConfig
    {
        private static TypeRegistry _typeRegistry;
        private static JsonParser _jsonParser;
        private static JsonFormatter _jsonFormatter;


        public static string BinaryUri => Descriptor.FullName;

        public static string JsonUri => Descriptor.FullName + "/Json";

        public static JsonParser JsonParser
        {
            get
            {
                TypeRegistryLazyInit();
                if (_jsonParser == null)
                    _jsonParser = new JsonParser(new JsonParser.Settings(16, _typeRegistry));

                return _jsonParser;
            }
        }

        public static JsonFormatter JsonFormatter
        {
            get
            {
                TypeRegistryLazyInit();
                if (_jsonFormatter == null)
                    _jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true, _typeRegistry));

                return _jsonFormatter;
            }
        }


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


        private static void TypeRegistryLazyInit()
        {
            if (_typeRegistry == null)
                _typeRegistry = TypeRegistry.FromFiles(RuntimePluginReflection.Descriptor);
        }
    }
}

using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.IO;

namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class PluginConfig
    {
        private static TypeRegistry _typeRegistry;
        private static JsonParser _jsonParser;
        private static JsonFormatter _jsonFormatter;


        public static PluginConfig LoadFromFile(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(file))
            {
                return ParseJson(reader);
            }
        }

        public static PluginConfig ParseJson(TextReader reader)
        {
            LazyInit();
            return _jsonParser.Parse<PluginConfig>(reader);
        }


        public void SaveToFile(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file))
            {
                FormatJson(writer);
            }
        }

        public void FormatJson(TextWriter writer)
        {
            LazyInit();
            _jsonFormatter.Format(this, writer);
        }

        public string ToJsonString()
        {
            LazyInit();
            return _jsonFormatter.Format(this);
        }


        private static void LazyInit()
        {
            if (_typeRegistry == null)
            {
                _typeRegistry = TypeRegistry.FromFiles(PluginInfoReflection.Descriptor, InputParameterConfigReflection.Descriptor, OutputParameterConfigReflection.Descriptor, ParameterConfigReflection.Descriptor);
                _jsonParser = new JsonParser(new JsonParser.Settings(16, _typeRegistry));
                _jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true, _typeRegistry));
            }
        }
    }
}
